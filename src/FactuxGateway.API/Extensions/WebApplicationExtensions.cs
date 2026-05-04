namespace FactuxGateway.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapGatewayProxy(this WebApplication app)
    {
        app.MapReverseProxy();
        return app;
    }
}
