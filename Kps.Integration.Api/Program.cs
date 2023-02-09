using System.Text.Json.Serialization;
using System.Diagnostics;

using Kps.Integration.Api;
using Kps.Integration.Api.Configs;
using Kps.Integration.Api.Infra;
using Kps.Integration.Proxy;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Kps.Integration.Api.Data;
using Kps.Integration.Api.HostedServices;
using Kps.Integration.Api.Controllers;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();



try
{
    
var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddControllers();

    builder.Services.AddHostedService<OrderKiotVietServices>();
    builder.Services.AddRazorPages();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    builder.Services
        .AddSwaggerGen()
        .AddProxies(builder.Configuration)
        .AddAppServices(builder.Configuration)
        .UseAutoMapper();

    builder.Services.AddDbContext<KpsIntegrationContext>(
        options =>
        {
            var sConnectionString = builder.Configuration.GetConnectionString("KpsIntegration");

            options.UseMySql(
                sConnectionString, Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(sConnectionString));
        });
    builder.Services.AddDbContext<integrationproddbContext>(
        options =>
        {
            options.UseMySql(builder.Configuration.GetConnectionString("IntegrationDB"),
            Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.23-mysql"));
        });
    
    builder.Services.UseAuth();
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSerilogRequestLogging(configure =>
    {
        configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    
    
    if (args == null || args.Length == 0 || string.IsNullOrEmpty(args[0]))
    {
        app.MapRazorPages();

        Log.Information("Starting web host");

        app.Run();
    }
    else
    {
        
        // Trace.Listeners.Add(new ConsoleTraceListener());
        var isMigrateJob = string.Equals(args.FirstOrDefault(), "migration", StringComparison.OrdinalIgnoreCase);
        //If argument is migration then run the migration and stop.
        if (isMigrateJob)
        {
            Trace.WriteLine("Started migration job");
            await HostExtensions.RunMigration();
            Trace.WriteLine("Completed migration job");
        }
        else
        {
            Log.Information($"Run background task on {DateTimeOffset.UtcNow}");

            await app.RunTaskAsync(args[0].ToHostTask());

            Log.Information($"Background task finished on {DateTimeOffset.UtcNow}");
        }
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    Log.Information($"Background task finished on {ex.Message}");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
