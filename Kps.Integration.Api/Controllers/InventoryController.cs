using Kps.Integration.Api.Extensions.Filters;
using Kps.Integration.Api.Models.Inventories;
using Kps.Integration.Api.Services;
using Kps.Integration.Proxy.CRM;
using Kps.Integration.Proxy.Magento;
using Microsoft.AspNetCore.Mvc;

namespace Kps.Integration.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly IMagentoService _magentoService;

        public InventoryController(ILogger<InventoryController> logger, IMagentoService magentoService)
        {
            _logger = logger;
            _magentoService = magentoService;
        }

        [ApiAuthentication]
        [HttpPost("add-quantity")]
        public async  Task<ActionResult> IncreaseItemQuantity(UpdateInventoryParams request)
        {
            try
            {
                var result = await _magentoService.AddInventory(request);
                if (result > 0)
                {
                    _logger.LogInformation("Add inventory {sku} - {qty}", request.Sku, request.Quantity);
                    return Ok();
                }

                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while adding inventory item {sku}, {qty}. {message} - {trace}", 
                    request.Sku, request.Quantity, ex.Message, ex.StackTrace);
                return BadRequest();
            }
        }
        
        [ApiAuthentication]
        [HttpPost("subtract-quantity")]
        public async  Task<ActionResult> DecreaseItemQuantity(UpdateInventoryParams request)
        {
            try
            {
                var result = await _magentoService.SubtractInventory(request);
                if (result > 0)
                {
                    _logger.LogInformation("Subtract inventory {sku} - {qty}", request.Sku, request.Quantity);
                    return Ok();
                }

                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while adding inventory item {sku}, {qty}. {message} - {trace}", 
                    request.Sku, request.Quantity, ex.Message, ex.StackTrace);
                return BadRequest();
            }
        }
    }
}