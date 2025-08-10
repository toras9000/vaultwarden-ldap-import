#!/usr/bin/env dotnet-script
#r "nuget: VwConnector, 1.34.3-rev.1"
#r "nuget: Lestaly.General, 0.102.0"
#r "nuget: Kokuban, 0.2.0"
#load ".settings.csx"
#nullable enable
using System.Text.Encodings.Web;
using System.Text.Json;
using Kokuban;
using Lestaly;
using Lestaly.Cx;
using VwConnector;
using VwConnector.Agent;

var settings = new
{
    VwliConfigFile = ThisSource.RelativeFile("./assets/vwli/settings.json"),
};

public record OperationSettings(int StartupWaitSeconds, int IntervalMinutes);
public record LdapServerSettings(string Host, ushort Port, bool Secure, int ProtocolVersion);
public record LdapCredeltialSettings(string Username, string Password);
public record LdapAttributesSettings(string ID, string Mail);
public record LdapDirectorySettings(string BaseDn, bool Subtree, string Filter, LdapAttributesSettings Attributes);
public record LdapMailaddrSettings(string[] AcceptPatterns);
public record LdapSettings(LdapServerSettings Server, LdapCredeltialSettings? Credential, LdapDirectorySettings Directory, LdapMailaddrSettings Mailaddr);
public record VaultwardenServerSettings(string Url);
public record VaultwardenOptionsSettings(bool OverwriteExisting);
public record VaultwardenOrganizationSettings(string OrgId, string ClientId, string ClientSecret);
public record VaultwardenClientSettings(string DeviceName, string DeviceIdentifier);
public record VaultwardenSettings(VaultwardenServerSettings Server, VaultwardenOptionsSettings Options, VaultwardenOrganizationSettings Organization, VaultwardenClientSettings Client);
public record AppSettings(OperationSettings Operation, LdapSettings Ldap, VaultwardenSettings Vaultwarden);

return await Paved.ProceedAsync(noPause: Args.RoughContains("--no-pause"), async () =>
{
    using var signal = new SignalCancellationPeriod();

    var testUser = vwSettings.Setup.TestUser;
    var testOrg = vwSettings.Setup.TestOrg;

    WriteLine("Prepare test user");
    using var agent = await VaultwardenAgent.CreateAsync(vwSettings.Service.Url, new(testUser.Mail, testUser.Password), signal.Token);
    var userProfile = agent.Profile;
    var orgProfile = agent.Profile.organizations.First(o => o.name == testOrg.Name);
    WriteLine($".. Created - {agent.Profile.id}");

    WriteLine("Get entities information");
    var userPassHash = agent.Connector.Utility.CreatePasswordHash(testUser.Mail, testUser.Password, agent.Kdf, hashIterations: 1);
    var orgApiKey = await agent.Connector.Organization.GetApiKeyAsync(agent.Token, orgProfile.id, new(userPassHash.EncodeBase64()), signal.Token);

    WriteLine("Update vaultwarden-ldap-import config");
    var config = await settings.VwliConfigFile.ReadJsonAsync<AppSettings>() ?? throw new Exception("Cannot load config");
    config = config with
    {
        Vaultwarden = config.Vaultwarden with
        {
            Organization = new(
                OrgId: orgProfile.id,
                ClientId: $"organization.{orgProfile.id}",
                ClientSecret: orgApiKey.apiKey
            ),
        },
    };

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    await settings.VwliConfigFile.WriteJsonAsync(config, options);

    WriteLine("Restart containers.");
    var composeFile = ThisSource.RelativeFile("./compose.yml");
    await "docker".args("compose", "--file", composeFile, "down", "importer");
    await "docker".args("compose", "--file", composeFile, "up", "-d", "--wait", "importer").result().success();

});
