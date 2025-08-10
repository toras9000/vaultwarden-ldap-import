# configuration file

vaultwarden-ldap-import はデフォルトで実行ファイルの場所からの相対パス `Data/settings.json` を設定ファイルとして読み込みます。  
この設定ファイルではLDAPやVaultwardenの接続情報などの設定を行います。  

## ファイル内容

ファイルはJSON形式で、全容は以下のようになります。  
個々の設定については後述します。  

```
{
  "Operation": {
    "StartupWaitSeconds": 60,
    "IntervalMinutes": 60
  },
  "Ldap": {
    "Server": {
      "Host": "ldap-server",
      "Port": 389,
      "Secure": false,
      "ProtocolVersion": 3
    },
    "Credential": {
      "Username": "user",
      "Password": "pass"
    },
    "Directory": {
      "BaseDn": "ou=accounts,dc=myserver,o=home",
      "Subtree": true,
      "Filter": "(&(objectClass=aaa)(memberOf=xxxx)(!(olcDisabled=TRUE)))",
      "Attributes": {
        "ID": "uid",
        "Mail": "email"
      }
    },
    "Mailaddr": {
      "AcceptPatterns": [
        "@example\\.com$"
      ]
    }
  },
  "Vaultwarden": {
    "Server": {
      "Url": "http://vaultwarden-server"
    },
    "Options": {
      "OverwriteExisting": false
    },
    "Organization": {
      "OrgId": "organization-id",
      "ClientId": "org-api-client-id",
      "ClientSecret": "org-api-client-secret"
    },
    "Client": {
      "DeviceName": "vaultwarden-ldap-import",
      "DeviceIdentifier": "vaultwarden-ldap-import"
    }
  }
}
```

## 設定の詳細

| パス                                    | 型           | 説明                                                                                                                                                                                                                                                         |
|-----------------------------------------|--------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Operation.StartupWaitSeconds`          | number       | 起動時の処理開始前待機時間[秒]                                                                                                                                                                                                                               |
| `Operation.IntervalMinutes`             | number       | 処理実行の間隔[分]                                                                                                                                                                                                                                           |
| `Ldap.Server.Host`                      | string       | LDAPサーバーのホスト名                                                                                                                                                                                                                                       |
| `Ldap.Server.Port`                      | number       | LDAPサーバーのポート番号                                                                                                                                                                                                                                     |
| `Ldap.Server.Secure`                    | boolean      | LDAPS（SSL/TLS）接続を有効化するか                                                                                                                                                                                                                           |
| `Ldap.Server.ProtocolVersion`           | number       | LDAPプロトコルバージョン                                                                                                                                                                                                                                     |
| `Ldap.Credential.Username`              | string       | LDAP認証ユーザー名。<br>指定されない場合は anonymous でアクセスする。                                                                                                                                                                                        |
| `Ldap.Credential.Password`              | string       | LDAP認証パスワード                                                                                                                                                                                                                                           |
| `Ldap.Directory.BaseDn`                 | string       | LDAP検索の基点となるBase DN                                                                                                                                                                                                                                  |
| `Ldap.Directory.Subtree`                | boolean      | サブツリー検索を有効化するか                                                                                                                                                                                                                                 |
| `Ldap.Directory.Filter`                 | string       | LDAP検索フィルター（RFC 4515形式）                                                                                                                                                                                                                           |
| `Ldap.Directory.Attributes.ID`          | string       | ユーザエントリの識別に利用する属性                                                                                                                                                                                                                           |
| `Ldap.Directory.Attributes.Mail`        | string       | ユーザエントリのメールアドレスを取得する属性                                                                                                                                                                                                                 |
| `Ldap.Mailaddr.AcceptPatterns`          | string array | 有効とするメールアドレスの正規表現パターン。<br>配列の先頭から順に評価し、最初に見つかったアドレスをインポートに用いる。マッチするものが見つからない場合はそのユーザエントリを無視する。<br>配列を空にした場合は、エントリの最初のメールアドレスを採用する。 |
| `Vaultwarden.Server.Url`                | string       | VaultwardenサーバーのURL                                                                                                                                                                                                                                     |
| `Vaultwarden.Options.OverwriteExisting` | boolean      | 既存データを上書きするか否か。<br>APIの要求パラメータ。                                                                                                                                                                                                      |
| `Vaultwarden.Organization.OrgId`        | string       | Vaultwardenの組織ID                                                                                                                                                                                                                                          |
| `Vaultwarden.Organization.ClientId`     | string       | 組織APIキーのクライアントID                                                                                                                                                                                                                                  |
| `Vaultwarden.Organization.ClientSecret` | string       | 組織APIキーのクライアントシークレット                                                                                                                                                                                                                        |
| `Vaultwarden.Client.DeviceName`         | string       | デバイスの表示名<br>これはVaultwardenで接続元の記録に用いられる。                                                                                                                                                                                            |
| `Vaultwarden.Client.DeviceIdentifier`   | string       | デバイス識別子。同上。                                                                                                                                                                                                                                       |
