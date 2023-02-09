using Kps.Integration.Api.Controllers;
using Kps.Integration.Api.Models.OrdersKiotViet;
using System.Timers;
using ConfigurationManager = Kps.Integration.Api.Models.OrdersKiotViet.ConfigurationManager;

namespace Kps.Integration.Api.HostedServices
{
    public class OrderKiotVietServices : IHostedService
    {
        private readonly KpsOrderKiotvietsController _controller;

        private readonly ILogger<OrderKiotVietServices> _logger;
        public OrderKiotVietServices(ILogger<OrderKiotVietServices> logger)
        {
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                         OnTimedEvent();
                        _logger.LogInformation("Task completed successfully");
                        await Task.Delay(new TimeSpan(0, 2, 0));
                    }
                });
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error schedule task!!!");
                throw;
            }
        }
        public async void OnTimedEvent()
        {
            try
            {
                var configManager = new ConfigurationManager();

                var endPointGet = configManager.GetConfigValue<string>("ApiKiotViet:urlOrderHttp");
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(endPointGet);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error call api!!!");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
