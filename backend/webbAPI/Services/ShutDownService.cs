using Microsoft.AspNetCore.SignalR;
using webbAPI.Hubs;

public class ShutdownService(IHostApplicationLifetime applicationLifetime, IHubContext<DrawHub> hubContext) : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime = applicationLifetime;
    private readonly IHubContext<DrawHub> _hubContext = hubContext;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
        return Task.CompletedTask;
    }

    private void OnApplicationStopping()
    {
        _hubContext.Clients.All.SendAsync("leaveGame");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Application has stopped.");
        return Task.CompletedTask;
    }
}

