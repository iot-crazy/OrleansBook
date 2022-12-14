using Microsoft.Extensions.Hosting;
using OrleansBook.GrainClasses;
using Orleans;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Orleans.Storage;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

Console.WriteLine("Starting Robot Server...");

var host = Host.CreateDefaultBuilder()
            .UseOrleans((ctx, silo) =>
            {
                silo.UseLocalhostClustering();
                silo.UseDashboard(opt => opt.HostSelf = true);

                silo.ConfigureLogging(logger =>
                 {
                     logger.AddConsole();
                     logger.SetMinimumLevel(LogLevel.Warning);
                 });

                silo.AddAzureTableGrainStorage(
                      name: "orleansbookstore",

                      configureOptions: options =>
                      {
                          options.TableName = "RobotGrains";
                          options.ConfigureTableServiceClient("UseDevelopmentStorage=true");
                      }
                      );
               // silo.AddBroadcastChannel("robotChannel",
                //    options => options.FireAndForgetDelivery = false);

                silo.AddBroadcastChannel("robotChannel", ob => ob.Configure(options => options.FireAndForgetDelivery = false));

                silo.AddAzureTableTransactionalStateStorage(
                    "TransactionStore", opt =>
                    {
                        opt.ConfigureTableServiceClient("UseDevelopmentStorage=true");
                    }
                   ).UseTransactions();


            })
            .Build();

await host.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

await host.StopAsync();