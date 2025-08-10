using System.Text.Json;
using Lestaly;
using Microsoft.Extensions.Logging;
using vaultwarden_ldap_import.Services;
using vaultwarden_ldap_import.Settings;

// 停止トークン作成
using var signal = new SignalCancellationPeriod();

// ロガーを作成
using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(conf => conf.SingleLine = true));
var logger = loggerFactory.CreateLogger("VWLI");

// 設定ファイル読み出し
var settingsFile = EnvVers.AppRelativeFile("VWLI_SETTINGS_FILE", "Data/settings.json");
logger.LogInformation($"Load settings file: {settingsFile.FullName}");
var settingsOptions = new JsonSerializerOptions { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, };
var settings = await settingsFile.ReadJsonAsync<AppSettings>(settingsOptions, cancelToken: signal.Token) ?? throw new Exception("Fialed to load settings");

// 未設定の状態を簡易的に検出
if (settings.Ldap.Server.Host.IsWhite() || settings.Vaultwarden.Server.Url.IsWhite())
{
    logger.LogError("No server settings");
    await Task.Delay(Timeout.Infinite, signal.Token);
    return;
}

// LDAPインポータ生成
using var importer = new ImportLdapService(settings, logger);

// 処理対象表示
logger.LogInformation($"Settings:");
logger.LogInformation($".. LDAP        : {settings.Ldap.Server.Host}:{settings.Ldap.Server.Port}");
logger.LogInformation($"   .. Login    : {(settings.Ldap.Credential?.Username).WhenWhite("anonymous")}");
logger.LogInformation($"   .. Search   : {settings.Ldap.Directory.BaseDn} ({(settings.Ldap.Directory.Subtree ? "OneLevel" : "Subtree")})");
logger.LogInformation($"   .. Filter   : {settings.Ldap.Directory.Filter}");
logger.LogInformation($".. Vaultwarden : {settings.Vaultwarden.Server.Url}");
logger.LogInformation($"   .. Org ID   : {settings.Vaultwarden.Organization.OrgId}");

// 起動時待機
if (0 < settings.Operation.StartupWaitSeconds)
{
    logger.LogInformation("Startup waiting...");
    var waitTime = TimeSpan.FromSeconds(settings.Operation.StartupWaitSeconds);
    await Task.Delay(waitTime, signal.Token);
}

// 定期的なインポート実行
var intervalTime = TimeSpan.FromMinutes(settings.Operation.IntervalMinutes);
while (true)
{
    try
    {
        await importer.ImportAsync(signal.Token);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to import");
    }
    await Task.Delay(intervalTime, signal.Token);
}
