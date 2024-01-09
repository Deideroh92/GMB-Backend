namespace GMB.ScannerService.Api.Services
{
    public class AuthorizationPolicyService
    {
        public string GetEnvironmentBasedPolicy()
        {
            // Assuming you set the environment name as an environment variable named "ASPNETCORE_ENVIRONMENT"
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase)
                ? "DevelopmentPolicy"
                : "ProductionPolicy";
        }
    }
}