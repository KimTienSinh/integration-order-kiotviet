namespace Kps.Integration.Api.Models.OrdersKiotViet
{
    public class ConfigurationManager
    {
        private readonly IConfiguration _config;

        public ConfigurationManager()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _config = builder.Build();
        }

        public T GetConfigValue<T>(string key)
        {
            return _config.GetValue<T>(key);
        }
    }
}
