using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansBook.GrainInterfaces;

Console.WriteLine("Simulating GPS positioning");

var host = Host.CreateDefaultBuilder()
                .UseOrleansClient((ctx, client) =>
                {
                    client.UseLocalhostClustering();
                    client.AddBroadcastChannel("robotChannel",
                    options => options.FireAndForgetDelivery = false);


                    // client.UseStaticClustering(new IPEndPoint(IPAddress.Loopback, 30000));

                    // client.UseAzureStorageClustering(options => options.ConfigureTableServiceClient(Environment.GetEnvironmentVariable("POIAzure")));
                })
                .ConfigureLogging(logger =>
                {
                    logger.AddConsole();
                    logger.SetMinimumLevel(LogLevel.Warning);
                })
                .Build();
host.Start();

var clusterClient = host.Services.GetRequiredService<IClusterClient>();


while (true)
{
    Console.WriteLine("Please enter a robot name:");
    var grainId = Console.ReadLine();

    var grain = clusterClient.GetGrain<IRobotGrain>(grainId);

    Console.WriteLine("Please enter an instruction...");
    var instruction = Console.ReadLine();
    if (string.IsNullOrEmpty(instruction) == false)
    {
        await grain.AddInstruction(instruction);
        var count = await grain.GetInstructionCount();
        Console.WriteLine($"{grainId} has {count} instructions.");
    }
}

/*Console.WriteLine("Press any key to exit...");
Console.ReadKey();
host.StopAsync();*/

