using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Erwin.Games.TreasureIsland.Persistence;
using Erwin.Games.TreasureIsland.Commands;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<IGameDataRepository, CosmosDbGameRepository>();
        services.AddHttpClient();
        services.AddSingleton<IAIClient, AIChatClient>();
    })
    .Build();

host.Run();
