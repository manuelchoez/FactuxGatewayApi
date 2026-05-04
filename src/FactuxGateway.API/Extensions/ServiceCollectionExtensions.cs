namespace FactuxGateway.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }
}
