using System.DirectoryServices.Protocols;
using System.Net;
using System.Text.RegularExpressions;
using Lestaly;
using Microsoft.Extensions.Logging;
using vaultwarden_ldap_import.Settings;
using VwConnector;

namespace vaultwarden_ldap_import.Services;

/// <summary>LDAPユーザのインポートを行う</summary>
public class ImportLdapService : IDisposable
{
    // 構築
    #region コンストラクタ
    /// <summary>依存データを受け取るコンストラクタ</summary>
    /// <param name="settings">設定</param>
    /// <param name="logger">ロガー</param>
    public ImportLdapService(AppSettings settings, ILogger logger)
    {
        this.logger = logger;
        this.appSettings = settings;
        this.addrPatterns = settings.Ldap.Mailaddr.AcceptPatterns.Select(p => new Regex(p)).ToArray();
        this.vwConnector = new VaultwardenConnector(new Uri(settings.Vaultwarden.Server.Url));
    }
    #endregion

    // 公開メソッド
    #region インポート処理
    /// <summary>LDAPユーザをVaultwardenにインポートする</summary>
    /// <param name="breaker">中止トークン</param>
    public async Task ImportAsync(CancellationToken breaker)
    {
        // LDAPのユーザを検索する
        this.logger.LogInformation("Search LDAP");
        var members = await loadLdapMembersAsync(breaker);
        if (members.Count <= 0)
        {
            this.logger.LogInformation(".. No result");
            return;
        }
        this.logger.LogInformation($".. Found {members.Count} members");

        // 前回値があれば変化を確認
        if (this.prevMembers != null)
        {
            var existDiff = this.prevMembers.Except(members).Any();
            if (!existDiff)
            {
                this.logger.LogInformation(".. No changes");
                return;
            }
        }

        // メンバーをインポートする
        this.logger.LogInformation("Import members");
        await importVwMembersAsync(members, breaker);
        this.logger.LogInformation(".. Completed");

        // 処理したメンバーを保持
        this.prevMembers = members;
    }
    #endregion

    #region インポート処理
    /// <summary>リソース破棄</summary>
    public void Dispose()
    {
        this.vwConnector.Dispose();
    }
    #endregion

    // 非公開型
    #region インポート処理
    /// <summary>LDAP検索結果用</summary>
    /// <param name="Id">LDAPユーザのID</param>
    /// <param name="Mail">LDAPユーザのメールアドレス</param>
    private record ImportMember(string Id, string Mail);
    #endregion

    // 非公開フィールド
    #region インポート処理
    /// <summary>設定</summary>
    private AppSettings appSettings;

    /// <summary>加工した設定</summary>
    private Regex[] addrPatterns;

    /// <summary>Vaultwardenアクセサ</summary>
    private VaultwardenConnector vwConnector;

    /// <summary>前回処理時のメンバー</summary>
    private List<ImportMember>? prevMembers;
    #endregion

    #region 依存サービス
    /// <summary>ロガー</summary>
    private ILogger logger;
    #endregion

    // 非公開メソッド
    #region 依存サービス
    /// <summary>LDAPユーザ情報を読み取る</summary>
    /// <param name="breaker">中止トークン</param>
    /// <returns>読みだしたユーザ情報</returns>
    private async Task<List<ImportMember>> loadLdapMembersAsync(CancellationToken breaker)
    {
        var settings = this.appSettings.Ldap;

        // LDAPサーバへの接続
        var server = new LdapDirectoryIdentifier(settings.Server.Host, settings.Server.Port);
        using var ldap = new LdapConnection(server);
        ldap.SessionOptions.SecureSocketLayer = settings.Server.Secure;
        ldap.SessionOptions.ProtocolVersion = settings.Server.ProtocolVersion;
        if ((settings.Credential?.Username).IsWhite())
        {
            ldap.AuthType = AuthType.Anonymous;
        }
        else
        {
            ldap.AuthType = AuthType.Basic;
            ldap.Credential = new NetworkCredential(settings.Credential.Username, settings.Credential.Password);
        }
        ldap.Bind();

        // ユーザの検索
        var members = new List<ImportMember>();
        var searchScope = settings.Directory.Subtree ? SearchScope.Subtree : SearchScope.OneLevel;
        var searchResult = await ldap.SearchAsync(settings.Directory.BaseDn, searchScope, settings.Directory.Filter, breaker);
        foreach (var entry in searchResult.Entries.OfType<SearchResultEntry>())
        {
            var id = (string?)entry.GetAttributeFirstValue(settings.Directory.Attributes.ID);
            var mails = entry.EnumerateAttributeValues(settings.Directory.Attributes.Mail).Select(m => (string?)m).ToArray();
            if (id == null || mails == null || mails.Length <= 0) continue;

            var mail = this.addrPatterns.Length <= 0
                     ? mails.FirstOrDefault()
                     : this.addrPatterns.SelectMany(p => mails.Where(m => m != null && p.IsMatch(m))).FirstOrDefault();
            if (mail.IsWhite()) continue;

            members.Add(new(id, mail));
        }

        return members;
    }

    /// <summary>Vaultwardenアクセス用認証トークンを取得する</summary>
    /// <param name="breaker">中止トークン</param>
    /// <returns>認証トークン</returns>
    private async ValueTask<ClientCredentialsConnectTokenResult> requestVwOrgTokenAsync(CancellationToken breaker)
    {
        var orgSettings = this.appSettings.Vaultwarden.Organization;
        var clientSettings = this.appSettings.Vaultwarden.Client;
        var orgCredential = new ClientCredentialsConnectTokenModel(
            scope: "api.organization",
            client_id: orgSettings.ClientId,
            client_secret: orgSettings.ClientSecret,
            device_type: ClientDeviceType.UnknownBrowser,
            device_name: clientSettings.DeviceName,
            device_identifier: clientSettings.DeviceIdentifier
        );

        var orgToken = await this.vwConnector.Identity.ConnectTokenAsync(orgCredential, breaker);
        return orgToken;
    }

    /// <summary>メンバをVaultwardenにインポートする</summary>
    /// <param name="members">インポート対象メンバー</param>
    /// <param name="breaker">中止トークン</param>
    private async Task importVwMembersAsync(List<ImportMember> members, CancellationToken breaker)
    {
        var orgToken = await requestVwOrgTokenAsync(breaker);

        var importArgs = new ImportOrgArgs(
            overwriteExisting: this.appSettings.Vaultwarden.Options.OverwriteExisting,
            members: members.Select(m => new ImportOrgMember(externalId: m.Id, email: m.Mail, deleted: false)).ToArray(),
            groups: []
        );

        await this.vwConnector.Public.ImportOrgMembersAsync(orgToken, importArgs, breaker);
    }
    #endregion
}
