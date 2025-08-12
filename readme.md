# vaultwarden-ldap-import

A tool for importing LDAP user information into Vaultwarden.  
It uses the public API to import users into a specified organization.

## How It Works

This tool operates according to the settings defined in the [configuration file](./docs/settings.md).  
At regular intervals, it retrieves user information from LDAP and imports the relevant users into Vaultwarden via the API.  
Only one set of retrieval conditions and one target organization can be specified.  
Post-import behavior within Vaultwarden depends on the API specifications.  
For example, if SMTP is enabled, an invitation email will be sent to the email addresses of imported users.  
This tool does not remove users. Even if a user can no longer be found in the LDAP search results, any Vaultwarden user that has been imported will remain unchanged.  

## Running the Tool

The executable is provided as a [Docker image](https://github.com/toras9000/vaultwarden-ldap-import/pkgs/container/vaultwarden-ldap-import).  
By default, the image’s executable looks for `/app/Data/settings.json` as the configuration file.  
You can override this path using the `VWLI_SETTINGS_FILE` environment variable.

Below is an example `docker-compose` configuration using an environment variable:

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