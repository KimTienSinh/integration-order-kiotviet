using Kps.Integration.Api.Services;
using Kps.Integration.Proxy.CRM;
using Kps.Integration.Proxy.Magento;
using Microsoft.AspNetCore.Mvc;

namespace Kps.Integration.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ILogger<HomeController> logger
        )
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "I'm running...";
        }
    }
}