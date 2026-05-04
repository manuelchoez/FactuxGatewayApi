namespace FactuxGateway.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddReverseProxyConfigurationFiles(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("reverseproxy.json", optional: false, reloadOnChange: true);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile("reverseproxy.Development.json", optional: true, reloadOnChange: true);
        }

        return builder;
    }
}
