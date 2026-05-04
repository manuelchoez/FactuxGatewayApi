namespace FactuxGateway.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayProxy(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var reverseProxySection = configuration.GetRequiredSection("ReverseProxy");

        ValidateReverseProxyConfiguration(reverseProxySection, environment);

        services
            .AddReverseProxy()
            .LoadFromConfig(reverseProxySection);

        return services;
    }

    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    return;
                }

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static void ValidateReverseProxyConfiguration(
        IConfigurationSection reverseProxySection,
        IHostEnvironment environment)
    {
        var routes = reverseProxySection.GetSection("Routes").GetChildren().ToArray();
        var clusters = reverseProxySection.GetSection("Clusters").GetChildren().ToArray();

        if (routes.Length == 0)
        {
            throw new InvalidOperationException("ReverseProxy configuration must define at least one route.");
        }

        if (clusters.Length == 0)
        {
            throw new InvalidOperationException("ReverseProxy configuration must define at least one cluster.");
        }

        foreach (var cluster in clusters)
        {
            var destinations = cluster.GetSection("Destinations").GetChildren().ToArray();

            if (destinations.Length == 0)
            {
                throw new InvalidOperationException(
                    $"ReverseProxy cluster '{cluster.Key}' must define at least one destination.");
            }

            foreach (var destination in destinations)
            {
                var address = destination["Address"];

                if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    throw new InvalidOperationException(
                        $"ReverseProxy destination '{cluster.Key}/{destination.Key}' must define a valid absolute address.");
                }

                if (!environment.IsDevelopment() && IsLocalAddress(uri))
                {
                    throw new InvalidOperationException(
                        $"ReverseProxy destination '{cluster.Key}/{destination.Key}' cannot use a local address outside Development.");
                }
            }
        }
    }

    private static bool IsLocalAddress(Uri uri)
    {
        return uri.IsLoopback
            || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }
}
