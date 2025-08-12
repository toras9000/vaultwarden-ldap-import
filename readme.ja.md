# vaultwarden-ldap-import

LDAPのユーザ情報を Vaultwarden にインポートするためのツールです。  
公開 API を利用し、指定した組織にユーザをインポートします。

## ツールの動作

本ツールは [設定ファイル](./docs/settings.ja.md) の内容に基づき、一定間隔で LDAP からユーザ情報を取得し、対象ユーザを Vaultwarden のAPI によりインポートします。  
読み取り条件およびインポート先の組織は、それぞれ 1 つのみ指定可能です。  
インポート後の Vaultwarden 上での挙動は API の仕様に依存します。  
たとえば SMTP 設定が有効な場合、インポートしたユーザのメールアドレス宛に招待メールが送信されるでしょう。  
このツールではユーザの削除を行いません。LDAPの検索結果からユーザが見つからなくなっても、一度インポートされたVaultwardenユーザはそのままです。  

## ツールの実行

実行バイナリは [Docker イメージ](https://github.com/toras9000/vaultwarden-ldap-import/pkgs/container/vaultwarden-ldap-import) として提供しています。  
このイメージは、デフォルトで `/app/Data/settings.json` を設定ファイルとして参照します。  
設定ファイルのパスは、環境変数 `VWLI_SETTINGS_FILE` で変更することもできます。

以下は、環境変数を利用した docker-compose の例です。

```yaml
services:
  importer:
    image: ghcr.io/toras9000/vaultwarden-ldap-import:0.1.0
    restart: unless-stopped
    volumes:
      - type: bind
        source: ./assets/vwli/settings.json
        target: /vwli/settings.json
        read_only: true
        bind:
          create_host_path: false
    environment:
      - VWLI_SETTINGS_FILE=/vwli/settings.json
```
