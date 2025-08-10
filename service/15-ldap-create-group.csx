#!/usr/bin/env dotnet-script
#r "nuget: Lestaly.General, 0.102.0"
#r "nuget: Lestaly.Ldap, 0.100.0"
#r "nuget: Kokuban, 0.2.0"
#load ".settings.csx"
#nullable enable
using System.DirectoryServices.Protocols;
using System.Net;
using Kokuban;
using Lestaly;

var settings = new
{
    DefaultMemberDn = "uid=authenticator,ou=operators,dc=myserver,o=home",
};

return await Paved.ProceedAsync(noPause: Args.RoughContains("--no-pause"), async () =>
{
    // Bind to LDAP server
    WriteLine("Bind to LDAP server");
    var server = new LdapDirectoryIdentifier(ldapSettings.Server.Host, ldapSettings.Server.Port);
    using var ldap = new LdapConnection(server);
    ldap.SessionOptions.SecureSocketLayer = ldapSettings.Server.Ssl;
    ldap.SessionOptions.ProtocolVersion = ldapSettings.Server.ProtocolVersion;
    ldap.AuthType = AuthType.Basic;
    ldap.Credential = ldapSettings.Credentials.DirectoryConfigurator;
    ldap.Bind();
    WriteLine(Chalk.Green[$".. OK"]);

    // Check group unit entry.
    WriteLine("Enter the name of the group to be created.");
    while (true)
    {
        Write(">");
        var name = ReadLine();
        if (name == null) break;
        if (name.IsWhite()) continue;

        try
        {
            var groupDn = $"cn={name},{ldapSettings.Directory.GroupUnitDn}";
            WriteLine(Chalk.Blue[$"Entity:"]);
            WriteLine(Chalk.Blue[$"  DN={groupDn}"]);
            WriteLine(Chalk.Blue[$"  cn={name}"]);

            await ldap.CreateEntryAsync(groupDn,
            [
                new("objectClass", "groupOfNames"),
                new("cn", name),
                new("member", settings.DefaultMemberDn),
            ]);
            WriteLine(Chalk.Green[$"Created: {groupDn}"]);
        }
        catch (Exception ex)
        {
            WriteLine(Chalk.Red[$"Error: {ex.Message}"]);
        }
        WriteLine();
    }
});
