using Orleans;
using Orleans.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseOrleans((ctx, silo) =>
{
    silo.UseLocalhostClustering();

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
});





// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
