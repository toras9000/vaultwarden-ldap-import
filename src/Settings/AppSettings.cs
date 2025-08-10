namespace vaultwarden_ldap_import.Settings;

/// <summary>動作設定</summary>
/// <param name="StartupWaitSeconds">開始時の待機時間[秒]</param>
/// <param name="IntervalMinutes">インポート処理間隔[分]</param>
public record OperationSettings(int StartupWaitSeconds, int IntervalMinutes);

/// <summary>LDAPサーバ設定</summary>
/// <param name="Host">ホスト</param>
/// <param name="Port">ポート</param>
/// <param name="Secure">TLSを利用するか否か</param>
/// <param name="ProtocolVersion">プロトコルバージョン</param>
public record LdapServerSettings(string Host, ushort Port, bool Secure, int ProtocolVersion);

/// <summary>LDAP認証情報</summary>
/// <param name="Username">ユーザ名</param>
/// <param name="Password">パスワード</param>
public record LdapCredeltialSettings(string Username, string Password);

/// <summary>LDAP検索ディレクトリ情報：属性情報</summary>
/// <param name="ID">LDAP上の識別ID</param>
/// <param name="Mail">ユーザのメールアドレス</param>
public record LdapAttributesSettings(string ID, string Mail);

/// <summary>LDAP検索ディレクトリ情報</summary>
/// <param name="BaseDn">ベースDN</param>
/// <param name="Subtree">サブツリーを検索するか否か</param>
/// <param name="Filter">検索フィルタ</param>
/// <param name="Attributes">属性情報</param>
public record LdapDirectorySettings(string BaseDn, bool Subtree, string Filter, LdapAttributesSettings Attributes);

/// <summary>LDAPメールアドレス設定</summary>
/// <param name="AcceptPatterns">受け入れるメールアドレスパターン</param>
public record LdapMailaddrSettings(string[] AcceptPatterns);

/// <summary>LDAP設定</summary>
/// <param name="Server">LDAPサーバ設定</param>
/// <param name="Credential">LDAP認証情報。指定が無い場合は anonymous アクセスとする。</param>
/// <param name="Directory">LDAP検索ディレクトリ情報</param>
/// <param name="Mailaddr">LDAPメールアドレス設定</param>
public record LdapSettings(LdapServerSettings Server, LdapCredeltialSettings? Credential, LdapDirectorySettings Directory, LdapMailaddrSettings Mailaddr);

/// <summary>Vaultwardenサーバ設定</summary>
/// <param name="Url">Vaultwarden URL</param>
public record VaultwardenServerSettings(string Url);

/// <summary>Vaultwardenオプション設定</summary>
/// <param name="OverwriteExisting">インポートユーザ</param>
public record VaultwardenOptionsSettings(bool OverwriteExisting);

/// <summary>Vaultwarden組織情報</summary>
/// <param name="OrgId">インポート先組織ID</param>
/// <param name="ClientId">インポート先組織 API クライアントID</param>
/// <param name="ClientSecret">インポート先組織 API クライアントシークレット</param>
public record VaultwardenOrganizationSettings(string OrgId, string ClientId, string ClientSecret);

/// <summary>Vaultwardenアクセスクライアント情報</summary>
/// <param name="DeviceName">デバイス名</param>
/// <param name="DeviceIdentifier">デバイス識別子</param>
public record VaultwardenClientSettings(string DeviceName, string DeviceIdentifier);

/// <summary>Vaultwarden設定</summary>
/// <param name="Server">Vaultwardenサーバ設定</param>
/// <param name="Server">Vaultwardenオプション設定</param>
/// <param name="Organization">Vaultwarden組織情報</param>
/// <param name="Client">Vaultwardenアクセスクライアント情報</param>
public record VaultwardenSettings(VaultwardenServerSettings Server, VaultwardenOptionsSettings Options, VaultwardenOrganizationSettings Organization, VaultwardenClientSettings Client);

/// <summary>アプリケーション設定のルート</summary>
/// <param name="Operation">動作設定</param>
/// <param name="Ldap">LDAP設定</param>
/// <param name="Vaultwarden">Vaultwarden設定</param>
public record AppSettings(OperationSettings Operation, LdapSettings Ldap, VaultwardenSettings Vaultwarden);
