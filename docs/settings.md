# Configuration File

By default, **vaultwarden-ldap-import** loads its configuration from the relative path `Data/settings.json`, based on the executable’s location.  
This configuration file defines settings such as LDAP and Vaultwarden connection details.

## File Contents

The file is in JSON format.  
Its full structure is shown below, with details for each setting described in the next section.

```json
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

## Setting Details

| Path                                   | Type         | Description                                                                                                                                                                                                                                                                                          |
|----------------------------------------|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Operation.StartupWaitSeconds`         | number       | Wait time in seconds before starting processing after startup.                                                                                                                                                                                                                                      |
| `Operation.IntervalMinutes`            | number       | Interval in minutes between processing runs.                                                                                                                                                                                                                                                        |
| `Ldap.Server.Host`                     | string       | LDAP server hostname.                                                                                                                                                                                                                                                                               |
| `Ldap.Server.Port`                     | number       | LDAP server port number.                                                                                                                                                                                                                                                                            |
| `Ldap.Server.Secure`                   | boolean      | Whether to enable LDAPS (SSL/TLS) connection.                                                                                                                                                                                                                                                       |
| `Ldap.Server.ProtocolVersion`          | number       | LDAP protocol version.                                                                                                                                                                                                                                                                              |
| `Ldap.Credential.Username`             | string       | LDAP bind username. <br>If omitted, the tool connects anonymously.                                                                                                                                                                                                                                  |
| `Ldap.Credential.Password`             | string       | LDAP bind password.                                                                                                                                                                                                                                                                                 |
| `Ldap.Directory.BaseDn`                | string       | Base DN used as the starting point for LDAP searches.                                                                                                                                                                                                                                               |
| `Ldap.Directory.Subtree`               | boolean      | Whether to enable subtree search.                                                                                                                                                                                                                                                                   |
| `Ldap.Directory.Filter`                | string       | LDAP search filter (RFC 4515 format).                                                                                                                                                                                                                                                                |
| `Ldap.Directory.Attributes.ID`         | string       | Attribute used to identify the user entry.                                                                                                                                                                                                                                                          |
| `Ldap.Directory.Attributes.Mail`       | string       | Attribute used to obtain the user entry’s email address.                                                                                                                                                                                                                                            |
| `Ldap.Mailaddr.AcceptPatterns`         | string array | Regular expression patterns for valid email addresses. <br>The array is evaluated in order, and the first matching address is used for import. If no match is found, the user entry is ignored. <br>If the array is empty, the first email address in the entry is used.                             |
| `Vaultwarden.Server.Url`               | string       | Vaultwarden server URL.                                                                                                                                                                                                                                                                             |
| `Vaultwarden.Options.OverwriteExisting`| boolean      | Whether to overwrite existing data. <br>This is passed as an API request parameter.                                                                                                                                                                                                                 |
| `Vaultwarden.Organization.OrgId`       | string       | Vaultwarden organization ID.                                                                                                                                                                                                                                                                        |
| `Vaultwarden.Organization.ClientId`    | string       | Client ID of the organization API key.                                                                                                                                                                                                                                                              |
| `Vaultwarden.Organization.ClientSecret`| string       | Client secret of the organization API key.                                                                                                                                                                                                                                                          |
| `Vaultwarden.Client.DeviceName`        | string       | Display name for the device. <br>This is used by Vaultwarden to record the source of the connection.                                                                                                                                                                                                |
| `Vaultwarden.Client.DeviceIdentifier`  | string       | Device identifier. Same usage as above.                                                                                                                                                                                                                                                             |
