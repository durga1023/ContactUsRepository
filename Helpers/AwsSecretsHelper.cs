using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using log4net;
using System.Text.Json;
using System.Threading.Tasks;

public static class AwsSecretsHelper
{
    private static readonly ILog log = LogManager.GetLogger(typeof(AwsSecretsHelper));

    public static async Task<string> GetSecretValueAsync(string secretName, string region)
    {
        log.Info($"Attempting to retrieve secret: {secretName} from region: {region}");

        var config = new AmazonSecretsManagerConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) };
        using var client = new AmazonSecretsManagerClient(config);

        try
        {
            var request = new GetSecretValueRequest { SecretId = secretName };
            var response = await client.GetSecretValueAsync(request);

            // If stored as a simple string
            if (!string.IsNullOrEmpty(response.SecretString))
            {
                log.Info("Successfully retrieved secret");

                // If you stored the whole connection string as the value
                return response.SecretString;
            }
            log.Error($"SecretString for '{secretName}' is empty.");
            throw new Exception("SecretString is empty.");

        }
        catch (Exception ex)
        {
            log.Error($"Failed to retrieve secret: {secretName}", ex);
            throw;
        }
    }

    public static async Task<string> GetSecretValueAsync(string secretName, string region, string key)
    {
        var secretJson = await AwsSecretsHelper.GetSecretValueAsync(secretName, region);
        var secretData = JsonSerializer.Deserialize<Dictionary<string, string>>(secretJson);
        return secretData[key];

    }
}
