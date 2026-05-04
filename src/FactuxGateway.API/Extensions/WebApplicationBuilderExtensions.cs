namespace FactuxGateway.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddReverseProxyConfigurationFiles(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("reverseproxy.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile(
            $"reverseproxy.{builder.Environment.EnvironmentName}.json",
            optional: true,
            reloadOnChange: true);

        return builder;
    }
}
