using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kps.Integration.Api.Data;
using Kps.Integration.Api.Models.OrdersKiotViet;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Configuration;
using ConfigurationManager = Kps.Integration.Api.Models.OrdersKiotViet.ConfigurationManager;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Numerics;

namespace Kps.Integration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KpsOrderKiotvietsController : ControllerBase
    {
        private readonly integrationproddbContext _context;

        public KpsOrderKiotvietsController(integrationproddbContext context)
        {
            _context = context;
        }

        [HttpGet("GetKpsOrderKiotvietsDB")]
        public async Task<ActionResult<IEnumerable<KpsOrderKiotviet>>> GetKpsOrderKiotvietsDB()
        {
            return await _context.KpsOrderKiotviets.ToListAsync();
        }

        
        private bool KpsOrderKiotvietExists(uint id)
        {
            return _context.KpsOrderKiotviets.Any(e => e.IdKps == id);
        }

        private async Task<string> callTokenAsync()
        {
            try
            {
                using var client = new HttpClient();
                var configManager = new ConfigurationManager();

                var endPoint = configManager.GetConfigValue<string>("KiotViet:urlToken");
                var grantType = configManager.GetConfigValue<string>("KiotViet:grant_type");
                var clientId = configManager.GetConfigValue<string>("KiotViet:client_id");
                var clientSecret = configManager.GetConfigValue<string>("KiotViet:client_secret");

                var data = new[]
                {
                    new KeyValuePair<string, string>("grant_type", grantType),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                };

                HttpResponseMessage response = await client.PostAsync(endPoint, new FormUrlEncodedContent(data));

                var stringResponse = await response.Content.ReadAsStringAsync();
                AuthenticatorToken res = (AuthenticatorToken)JsonConvert.DeserializeObject(stringResponse, typeof(AuthenticatorToken));
                return res.access_token;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpGet("GetListOrderKiotviet")]
        public async Task<List<KpsOrderKiotviet>> GetListOrderKiotviet()
        {
            try
            {
                using var client = new HttpClient();
                var token = callTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await token);
                var configManager = new ConfigurationManager();
                var endPoint = configManager.GetConfigValue<string>("KiotViet:OrderKiotViet");
                client.DefaultRequestHeaders.Add("Retailer", "kpsmall");
                var response = await client.GetStringAsync(endPoint);


                var content = JsonConvert.DeserializeObject<JToken>(response);

                DateTime dateTime = DateTime.Now;

                string timeNowString = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");

                var orderIdDbMax = _context.KpsOrderKiotviets.Max(o => o.Id);

                var listOrderKiotvietUnsync = content.SelectTokens("data[*]")
                  .Select(order => new KpsOrderKiotviet
                  {
                      Id = (Int64)order["id"],
                      Code = (string)order["code"],
                      SoldByName = (string)order["soldByName"],
                      CustomerCode = (string)order["customerCode"],
                      CustomerName = (string)order["customerName"],
                      Total = (double)order["total"],
                      TotalPayment = (double)order["totalPayment"],
                      CreatedDate = (DateTime)order["createdDate"],
                      CreatedAt = DateTime.Parse(timeNowString)

                  }).ToList().Where(o => o.Id > orderIdDbMax.Value);


                return listOrderKiotvietUnsync.ToList();

            }
            catch (Exception ex)
            {
                throw;

            }
        }


        [HttpPost("SyncListOrderPost")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<ActionResult> SyncListOrderPost([FromBody] List<KpsOrderKiotviet> listOrder)
        {
            listOrder = await GetListOrderKiotviet();
            _context.KpsOrderKiotviets.AddRange(listOrder);
            _context.SaveChanges();

            return CreatedAtAction("SyncListOrderPost", new { }, listOrder);
        }

/*
        [HttpGet("SyncListOrderHosted")]
        public async Task<ActionResult> SyncListOrderHosted()
        {
          //  var syncOrderHosted = await SyncListOrderPost();
            var orderIdMax = _context.KpsOrderKiotviets
                .Max(o => o.Id);

            return Ok(orderIdMax);

        }*/
    }
}
