using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.Commons;

public class VaultService
{
    private readonly IVaultClient _vaultClient;

    public VaultService(IConfiguration configuration)
    {
        var vaultConfig = configuration.GetSection("Vault");
        var authMethod = vaultConfig.GetValue<string>("AuthenticationMethod");
        var address = vaultConfig.GetValue<string>("Address");

        IAuthMethodInfo authMethodInfo;

        if (authMethod == "AppRole")
        {
            var roleId = Environment.GetEnvironmentVariable("VAULT_ROLE_ID");
            var secretId = Environment.GetEnvironmentVariable("VAULT_SECRET_ID");

            if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(secretId))
            {
                throw new ArgumentException("RoleId and SecretId must be set in environment variables for AppRole authentication.");
            }

            authMethodInfo = new AppRoleAuthMethodInfo(roleId, secretId);
        }
        else if (authMethod == "Kubernetes")
        {
            var roleName = vaultConfig.GetValue<string>("Kubernetes:RoleName");
            var jwt = File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
            authMethodInfo = new KubernetesAuthMethodInfo(roleName, jwt);
        }
        else
        {
            throw new ArgumentException("Invalid authentication method specified in configuration.");
        }

        var vaultClientSettings = new VaultClientSettings(address, authMethodInfo);
        _vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task<string> GetSecretAsync(string secretPath)
    {
        var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath);

        if (secret?.Data?.Data != null && secret.Data.Data.TryGetValue("mysecret", out var mySecret))
        {
            return mySecret?.ToString() ?? string.Empty;
        }
        else
        {
            throw new Exception("Secret or expected key 'mysecret' not found.");
        }
    }

}
