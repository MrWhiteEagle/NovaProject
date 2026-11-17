using NovaProject.RelayServer.Routing;
using NovaProject.RelayServer.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ServerIdentityProvider>();
builder.Services.AddSingleton<RelaySessionRegistry>();
builder.Services.AddTransient<ClientConnectionHandler>();

var app = builder.Build();

var identity = app.Services.GetRequiredService<ServerIdentityProvider>();
Console.WriteLine("=================================================");
Console.WriteLine("           NovaApp Relay Server Online           ");
Console.WriteLine($"   Server Fingerprint: {identity.Fingerprint}   ");
Console.WriteLine("      Listening on: ws://localhost:5050/ws       ");
Console.WriteLine("=================================================");

app.UseWebSockets();

app.Map("/ws", async (HttpContext context, ClientConnectionHandler handler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    
    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    await handler.HandleConnectionAsync(webSocket, context.RequestAborted);
});

app.Run("http://localhost:5050");