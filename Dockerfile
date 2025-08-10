FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /work
COPY ./  ./

RUN dotnet publish src -o ./publish


FROM mcr.microsoft.com/dotnet/runtime:9.0

COPY --from=builder /work/publish          /app

RUN <<-EOL
    apt-get update
    apt-get install -y --no-install-recommends libldap-2.5-0
    apt-get clean
    rm -rf /var/lib/apt/lists/*
EOL

CMD ["app/vaultwarden-ldap-import"]
