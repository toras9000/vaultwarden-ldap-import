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
    WriteLine("Enter the uid of the person to be created.");
    while (true)
    {
        Write(">");
        var uid = ReadLine();
        if (uid == null) break;
        if (uid.IsWhite()) continue;

        try
        {
            var cn = uid;
            var sn = uid;
            var mail = $"{uid}@myserver.home";
            var passwd = $"{uid}-pass";
            var personDn = $"uid={uid},{ldapSettings.Directory.PersonUnitDn}";
            WriteLine(Chalk.Blue[$"Entity:"]);
            WriteLine(Chalk.Blue[$"  DN={personDn}"]);
            WriteLine(Chalk.Blue[$"  cn={cn}"]);
            WriteLine(Chalk.Blue[$"  sn={sn}"]);
            WriteLine(Chalk.Blue[$"  mail={mail}"]);
            WriteLine(Chalk.Blue[$"  password={passwd}"]);

            var hash = LdapExtensions.MakePasswordHash.SSHA256(passwd);
            await ldap.CreateEntryAsync(personDn,
            [
                new("objectClass", ["inetOrgPerson", "extensibleObject"]),
                new("cn", cn),
                new("sn", sn),
                new("mail", mail),
                new("userPassword", hash),
            ]);
            WriteLine(Chalk.Green[$"Created: {personDn}"]);
        }
        catch (Exception ex)
        {
            WriteLine(Chalk.Red[$"Error: {ex.Message}"]);
        }
        WriteLine();
    }
});
