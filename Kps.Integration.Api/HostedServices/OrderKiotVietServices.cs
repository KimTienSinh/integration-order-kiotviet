using Kps.Integration.Api.Controllers;
using Kps.Integration.Api.Models.OrdersKiotViet;
using System.Timers;
using ConfigurationManager = Kps.Integration.Api.Models.OrdersKiotViet.ConfigurationManager;

namespace Kps.Integration.Api.HostedServices
{
    public class OrderKiotVietServices : IHostedService
    {
        KpsOrderKiotvietsController _controller;

        

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                  // await _controller.SyncListOrderPost();
                    await Task.Delay(new TimeSpan(0, 5, 0));
                }
            });
            return Task.CompletedTask;
        }
        /*private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var configManager = new ConfigurationManager();
            var endPoint = configManager.GetConfigValue<string>("KiotViet:urlToken");
            using (var httpClient = new HttpClient())
            {
                var order = GetListOrderKiotviet();
                var response = await httpClient.PostAsJsonAsync("https://example.com/api/Orders", order);
                response.EnsureSuccessStatusCode();
            }
        }

        public Task<List<KpsOrderKiotviet>> GetListOrderKiotviet()
        {
            var listOrder = _controller.GetListOrderKiotviet();
            return listOrder;
        }*/

        //https://localhost:7150/api/KpsOrderKiotviets/SyncListOrderPost

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
