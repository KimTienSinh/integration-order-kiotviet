using Kps.Integration.Api.Flows;
using System.Diagnostics;
using Kps.Integration.Api.Infra;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Kps.Integration.Api
{
    public enum HostTaskName
    {
        SyncOrderFromMagentoToCRMFlow,
        SyncOrderFromMagentoToWMSFlow,
        ReSyncOrderFromMagentoToWMSFlow,
        SyncInventoryToMagento
    }

    public static class HostExtensions
    {
        public static async Task RunTaskAsync(this WebApplication host, HostTaskName taskName)
        {
            Log.Information($"Start handle task {taskName}");

            switch (taskName)
            {
                case HostTaskName.SyncOrderFromMagentoToCRMFlow:
                    await host.SyncOrderFromMagentoToCRMAsync().ConfigureAwait(false);
                    Log.Information($"Complete handle task {taskName}");
                    break;
                case HostTaskName.SyncOrderFromMagentoToWMSFlow:
                    await host.SyncOrderFromMagentoToWmsAsync().ConfigureAwait(false);
                    Log.Information($"Complete handle task {taskName}");
                    break;
                case HostTaskName.ReSyncOrderFromMagentoToWMSFlow:
                    await host.ReSyncOrderFromMagentoToWmsAsync().ConfigureAwait(false);
                    Log.Information($"Complete handle task {taskName}");
                    break;
                case HostTaskName.SyncInventoryToMagento:
                    await host.SyncInventoryToMagento().ConfigureAwait(false);
                    Log.Information($"Complete handle task {taskName}");
                    break;
            }
        }
        
        

        public static async Task RunMigration()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile($"appsettings.{env}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<KpsIntegrationContext>();
            
            var sConnectionString = config.GetConnectionString("KpsIntegration");
            builder.UseMySql(
                sConnectionString, ServerVersion.AutoDetect(sConnectionString));
            
            await using var db = new KpsIntegrationContext(builder.Options);
            await db.Database.MigrateAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// This task will sync data for mid-market rate from our partner 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static async Task SyncOrderFromMagentoToCRMAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var action = scope.ServiceProvider.GetRequiredService<ISyncMagentoOrderToCrmFlow>();

            var result = await action.ExecuteAsync(100).ConfigureAwait(false);

            Log.Information(
                $"Successful run {nameof(SyncOrderFromMagentoToCRMAsync)} on {DateTimeOffset.UtcNow} with the result {result}");
        }
        
        public static async Task SyncOrderFromMagentoToWmsAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var action = scope.ServiceProvider.GetRequiredService<ISyncOrderFromMagentoToWmsIntegrationFlow>();

            var result = await action.ExecuteAsync(100).ConfigureAwait(false);

            Log.Information(
                $"Successful run {nameof(SyncOrderFromMagentoToWmsAsync)} on {DateTimeOffset.UtcNow} with the result {result}");
        }
        
        public static async Task ReSyncOrderFromMagentoToWmsAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var action = scope.ServiceProvider.GetRequiredService<IReSyncOrderFromMagentoToWmsIntegrationFlow>();

            var result = await action.ExecuteAsync(100).ConfigureAwait(false);

            Log.Information(
                $"Successful run {nameof(ReSyncOrderFromMagentoToWmsAsync)} on {DateTimeOffset.UtcNow} with the result {result}");
        }

        public static async Task SyncInventoryToMagento(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var action = scope.ServiceProvider.GetRequiredService<ISyncInventoryToMagento>();

            var result = await action.ExecuteAsync("KPS").ConfigureAwait(false);

            Log.Information(
                $"Successful run {nameof(ReSyncOrderFromMagentoToWmsAsync)} on {DateTimeOffset.UtcNow} with the result {result}");
        }
    }

    internal static class HostTaskStringExtensions
    {
        public static HostTaskName ToHostTask(this string hostTask)
        {
            return Enum.TryParse(hostTask, true, out HostTaskName result)
                ? result
                : throw new ArgumentException($"invalid task name {nameof(hostTask)}");
        }
    }
}
