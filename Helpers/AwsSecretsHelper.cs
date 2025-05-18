using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Threading.Tasks;

public static class AwsSecretsHelper
{
    public static async Task<string?> GetSecretAsync(string secretName, string region = "us-east-2")
    {
        using var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
        var request = new GetSecretValueRequest { SecretId = secretName };
        var response = await client.GetSecretValueAsync(request);
        return response.SecretString;
    }
}
