using FactuxGateway.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddReverseProxyConfigurationFiles();

builder.Services.AddGatewayProxy(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGatewayProxy();

app.Run();
