using Astra.Models;
using Astra.Database.Seeders;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration, builder.Host);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope()) {
    var services = scope.ServiceProvider;

    try {
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.Initialize(services);
    } catch(Exception e) {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(e, "An error occurred while seeding the database");
    }
}

startup.Configure(app, app.Environment);
app.MapOpenApi();
app.MapControllers();
app.Run();