using FactuxGateway.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddReverseProxyConfigurationFiles();

builder.Services.AddGatewayProxy(builder.Configuration);

var app = builder.Build();

app.MapGatewayProxy();

app.Run();