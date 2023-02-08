using AutoMapper;
using Kps.Integration.Api.Infra;
using Kps.Integration.Api.Services;
using Kps.Integration.Domain.Constants;
using Kps.Integration.Proxy.CRM;
using Kps.Integration.Proxy.Magento;

namespace Kps.Integration.Api.Flows.Concrete;

public class SyncMagentoOrderToCrmFlow : ISyncMagentoOrderToCrmFlow
{
    protected readonly IGetflyProxy _pxyGetfly;
    protected readonly ILogger<SyncMagentoOrderToCrmFlow> _logger;
    protected readonly IMagentoProxy _pxyMagento;
    protected readonly IMagentoService _svcMagento;
    protected readonly IMapper _mapper;
    protected readonly KpsIntegrationContext _dbContext;

    public SyncMagentoOrderToCrmFlow(
        IGetflyProxy pxyGetfly,
        ILogger<SyncMagentoOrderToCrmFlow> logger,
        IMagentoProxy pxyMagento,
        IMagentoService svcMagento,
        IMapper mapper,
        KpsIntegrationContext dbContext
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
        _pxyGetfly = pxyGetfly;
        _pxyMagento = pxyMagento;
        _svcMagento = svcMagento;
    }

    public async Task<bool> ExecuteAsync(int iLimit)
    {
        try
        {
            var dtmLastOrderOn = GetLastOrderTime();

            var aSalesOrder = await _svcMagento.GetOrderList(dtmLastOrderOn, iLimit);

            int i;

            if (aSalesOrder == null || (i = aSalesOrder.Length) == 0)
            {
                _logger.LogInformation("Cannot find any new orders to process... Stopping Magento to CRM flow");

                return false;
            }

            var aOrder = new Domain.Entities.Order[i];

            i = 0;

            foreach (var x in aSalesOrder)
            {
                var enOrder = new Domain.Entities.Order
                {
                    CreatedOn = System.DateTime.Now,
                    OrderCreatedOn = x.CreatedAt,
                    OrderId = x.EntityId,
                    Status = Domain.Constants.OrderStatus.Init
                };

                _dbContext.Order.Add(enOrder);

                aOrder[i++] = enOrder;
            }

            _dbContext.SaveChanges();

            var aGetflyOrder = await PrepareCrmOrder(aOrder);

            if (aGetflyOrder == null)
            {
                _logger.LogInformation("Failed (not found any order from MagentoApi)");

                return false;
            }

            await PrepareCrmAccount(aGetflyOrder);

            await PrepareCrmProduct(aGetflyOrder);

            await SyncToCrm(aGetflyOrder);

            i -= 1;

            _dbContext.ScheduleLogging.Add(
                new Domain.Entities.ScheduleLogging
                {
                    CreatedOn = System.DateTime.Now,
                    LastOrderTime = aSalesOrder[i].CreatedAt,
                    ApplicationName = ApplicationName.GET_FLY
                }
            );

            _dbContext.SaveChanges();

            var resultData = new Models.Orders.SyncRespData
            {
                LastOrderId = aSalesOrder[i].EntityId
            };

            resultData.LastOrderOn = aSalesOrder[i].CreatedAt.ToString("yyyy-MM-dd HH:mms:ss");

            _logger.LogInformation($"Sync order data from Magento to CRM successfully at {DateTimeOffset.Now}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Cannot sync orders to Magento to CRM at {DateTimeOffset.Now}.");

            return false;
        }
    }

    #region Manipulations

    protected async Task PrepareCrmAccount(Proxy.CRM.Models.Orders.CreateOrderReq[] requests)
    {
        var iRequestCount = 0;

        var lstCustomerId = new List<int>();

        foreach (var req in requests)
        {
            lstCustomerId.Add(req.OrderInfo.CustomerId);

            iRequestCount += 1;
        }

        // xac dinh account_code tu CustomerMapping
        var arr = lstCustomerId.Distinct().ToArray();

        var lstCustomerMapping = _dbContext
            .CustomerMapping.Where(x => arr.Contains(x.CustomerId) == true).ToList();

        var i = 0;

        if (lstCustomerMapping != null && lstCustomerMapping.Count != 0)
        {
            foreach (var mapping in lstCustomerMapping)
            {
                foreach (var req in requests)
                {
                    if (req.OrderInfo.CustomerId == mapping.CustomerId)
                    {
                        req.OrderInfo.AccountCode = mapping.GetflyCustomerCode;
                        req.OrderInfo.HasMappedAccount = true;

                        i += 1;
                    }
                }
            }
        }

        // da map het account_code
        if (i == iRequestCount)
        {
            return;
        }

        var lstGetflyAccountKeyword = new List<string>();

        i = 0;

        foreach (var x in requests)
        {
            if (x.OrderInfo.HasMappedAccount == false)
            {
                if (x.OrderInfo.HasAccountEmail == true)
                {
                    lstGetflyAccountKeyword.Add(x.OrderInfo.AccountEmail);

                    i += 1;
                }

                if (x.OrderInfo.HasAccountPhone == true)
                {
                    lstGetflyAccountKeyword.Add(x.OrderInfo.AccountPhone);

                    i += 1;
                }
            }
        }

        // lay danh sach account tu Getfly
        if (i != 0)
        {
            lstGetflyAccountKeyword = lstGetflyAccountKeyword.Distinct().ToList();

            var lstTask1 = lstGetflyAccountKeyword.Select(GetCrmAccount).ToList();

            var aGetflyAccount1 = await Task.WhenAll(lstTask1);

            if (aGetflyAccount1 != null && aGetflyAccount1.Length != 0)
            {
                i = 0;

                if (lstCustomerMapping == null)
                {
                    lstCustomerMapping = new List<Domain.Entities.CustomerMapping>();
                }
                else
                {
                    lstCustomerMapping.Clear();
                }

                foreach (var x in aGetflyAccount1)
                {
                    if (x != null)
                    {
                        foreach (var y in requests)
                        {
                            if (y.OrderInfo.HasMappedAccount == false)
                            {
                                if (y.OrderInfo.AccountEmail.Equals(x.Email) == true
                                    ||
                                    y.OrderInfo.AccountPhone.Equals(x.Phone) == true)
                                {
                                    y.OrderInfo.AccountCode = x.AccountCode;
                                    y.OrderInfo.HasMappedAccount = true;

                                    lstCustomerMapping.Add(
                                        new Domain.Entities.CustomerMapping
                                        {
                                            CustomerEmail = y.OrderInfo.AccountEmail,
                                            CustomerId = y.OrderInfo.CustomerId,
                                            CustomerPhone = x.Phone,
                                            GetflyCustomerCode = x.AccountCode,
                                            GetflyCustomerId = x.AccountId
                                        }
                                    );

                                    i += 1;
                                }
                            }
                        }
                    }
                }

                // map duoc it nhat 1 account_code
                if (i != 0)
                {
                    i = requests.Count(x => x.OrderInfo.HasMappedAccount);

                    lstCustomerId.Clear();

                    foreach (var x in lstCustomerMapping.Where(x => lstCustomerId.Contains(x.CustomerId) == false))
                    {
                        _dbContext.CustomerMapping.Add(x);

                        lstCustomerId.Add(x.CustomerId);
                    }

                    _dbContext.SaveChanges();

                    // da map het account_code
                    if (i == iRequestCount)
                    {
                        return;
                    }
                }
            }
        }

        // tao account o Getfly
        var lstGetflyAccount2 = new List<Proxy.CRM.Models.Accounts.Account>();

        i = 0;

        foreach (var x in requests)
        {
            if (x.OrderInfo.HasMappedAccount == false)
            {
                if (x.OrderInfo.HasAccountEmail == true
                    ||
                    x.OrderInfo.HasAccountPhone == true)
                {
                    var account =
                        _mapper.Map<Proxy.CRM.Models.Accounts.Account>(x.OrderInfo);

                    lstGetflyAccount2.Add(account);

                    i += 1;
                }
            }
        }

        if (i != 0)
        {
            var lstTask2 = new List<Task<Proxy.CRM.Models.Accounts.CreateAccountResp>>();

            foreach (var x in lstGetflyAccount2)
            {
                lstTask2.Add(CreateCrmAccount(x));
            }

            var lst = await Task.WhenAll(lstTask2);

            i = 0;

            foreach (var x in lst)
            {
                if (x.Code != Proxy.CRM.Constants.ResultCode.CreateSucceed)
                {
                    continue;
                }

                foreach (var y in requests)
                {
                    if (y.OrderInfo.HasMappedAccount == false
                        &&
                        y.OrderInfo.CustomerId == x.MagentoCustomerId)
                    {
                        y.OrderInfo.AccountCode = x.AccountCode;

                        break;
                    }
                }

                _dbContext.CustomerMapping.Add(
                    new Domain.Entities.CustomerMapping
                    {
                        CustomerEmail = x.AccountEmail,
                        CustomerId = x.MagentoCustomerId,
                        CustomerPhone = x.AccountPhone,
                        GetflyCustomerCode = x.AccountCode,
                        GetflyCustomerId = x.AccountId
                    }
                );

                i += 1;
            }

            if (i != 0)
            {
                _dbContext.SaveChanges();
            }
        }
    }

    protected async Task PrepareCrmProduct(Proxy.CRM.Models.Orders.CreateOrderReq[] requests)
    {
        var lstProductId = new List<int>();

        foreach (var x in requests)
        {
            if (x.Products != null)
            {
                foreach (var y in x.Products)
                {
                    lstProductId.Add(y.MagentoProductId);
                }
            }
        }

        var arr = lstProductId.Distinct().ToArray();

        var aProductMapping = _dbContext
            .ProductMapping.Where(x => arr.Contains(x.ProductId)).ToArray();

        if (aProductMapping != null && aProductMapping.Length != 0)
        {
            foreach (var x in aProductMapping)
            {
                foreach (var y in requests)
                {
                    if (y.Products != null)
                    {
                        foreach (var z in y.Products)
                        {
                            if (z.MagentoProductId == x.ProductId)
                            {
                                z.ProductCode = x.GetflyProductCode;
                                z.ProductId = x.GetflyProductId;
                            }
                        }
                    }
                }
            }
        }

        lstProductId.Clear();

        var i = 0;

        foreach (var x in requests)
        {
            if (x.Products != null)
            {
                foreach (var y in x.Products)
                {
                    if (string.IsNullOrEmpty(y.ProductCode) == true)
                    {
                        lstProductId.Add(y.MagentoProductId);

                        i += 1;
                    }
                }
            }
        }

        // da map het product_code
        if (i == 0)
        {
            return;
        }

        lstProductId = lstProductId.Distinct().ToList();

        var lstTask1 = new List<Task<Proxy.CRM.Models.Products.Product>>();

        foreach (var x in lstProductId)
        {
            lstTask1.Add(GetCrmProduct(x));
        }

        var lstGetflyProduct1 = await Task.WhenAll(lstTask1);

        if (lstGetflyProduct1 != null)
        {
            i = 0;

            foreach (var x in lstGetflyProduct1)
            {
                if (x != null)
                {
                    foreach (var y in requests)
                    {
                        if (y.Products != null)
                        {
                            foreach (var z in y.Products)
                            {
                                if (z.MagentoProductId == x.MagentoProductId)
                                {
                                    z.ProductCode = x.ProductCode;
                                    z.ProductId = x.ProductId;
                                }
                            }
                        }
                    }

                    _dbContext.ProductMapping.Add(
                        new Domain.Entities.ProductMapping
                        {
                            GetflyProductCode = x.ProductCode,
                            GetflyProductId = x.ProductId,
                            ProductId = x.MagentoProductId
                        }
                    );

                    i += 1;
                }
            }

            if (i != 0)
            {
                _dbContext.SaveChanges();
            }
        }

        i = 0;

        foreach (var x in requests)
        {
            if (x.Products != null)
            {
                foreach (var y in x.Products)
                {
                    if (string.IsNullOrEmpty(y.ProductCode) == true)
                    {
                        i += 1;
                    }
                }
            }
        }

        // da map het product_code
        if (i == 0)
        {
            return;
        }

        var lstTask2 = new List<Task<Proxy.CRM.Models.Products.CreateProductResp>>();

        foreach (var x in lstProductId)
        {
            lstTask2.Add(CreateCrmProduct(x));
        }

        var lstGetflyProduct2 = await Task.WhenAll(lstTask2);

        i = 0;


        foreach (var x in lstGetflyProduct2)
        {
            if (x.ProductId == 0)
            {
                continue;
            }

            foreach (var y in requests)
            {
                if (y.Products != null)
                {
                    foreach (var z in y.Products)
                    {
                        if (z.MagentoProductId == x.MagenoProductId)
                        {
                            z.ProductCode = x.ProductCode;
                            z.ProductId = x.ProductId;
                        }
                    }
                }
            }

            _dbContext.ProductMapping.Add(
                new Domain.Entities.ProductMapping
                {
                    GetflyProductCode = x.ProductCode,
                    GetflyProductId = x.ProductId,
                    ProductId = x.MagenoProductId
                }
            );

            i += 1;
        }

        if (i != 0)
        {
            _dbContext.SaveChanges();
        }
    }

    protected async Task<Proxy.CRM.Models.Orders.CreateOrderReq[]> PrepareCrmOrder(params Domain.Entities.Order[] orders)
    {
        var lstTask = new List<Task<Proxy.Magento.Models.Orders.GetOrderResp>>();

        foreach (var x in orders)
        {
            lstTask.Add(GetMagentoOrder(x.OrderId));
        }

        var arr = await Task.WhenAll(lstTask);

        if (arr == null || arr.Length == 0)
        {
            _logger.LogInformation("Not found any order from MagentoApi");

            return null;
        }

        var i = 0;

        var bCanSkip = false;

        var sb = new System.Text.StringBuilder();

        var lstReq = new List<Proxy.CRM.Models.Orders.CreateOrderReq>();

        foreach (var x in arr)
        {
            bCanSkip = true;

            var enOrder = _dbContext.Order.Where(y => y.OrderId == x.OrderId).First();

            if (x.Order == null)
            {
                enOrder.Status = Domain.Constants.OrderStatus.FailedByMagentoApi;
            }
            else
            {
                enOrder.CustomerId = x.Order.CustomerId;

                if (x.Order.ExtensionAttributes == null
                    ||
                    x.Order.ExtensionAttributes.ShippingAssignments == null
                    ||
                    x.Order.ExtensionAttributes.ShippingAssignments.Length == 0)
                {
                    enOrder.Status = Domain.Constants.OrderStatus.InvalidDataFromMagentoApi;
                }
                else
                {
                    bCanSkip = false;
                }

                if (string.IsNullOrEmpty(x.ResponsePayload) == false)
                {
                    enOrder.MagentoPayload = GetBytes(x.ResponsePayload);
                }
            }

            _dbContext.Order.Update(enOrder);

            _dbContext.SaveChanges();

            if (bCanSkip == true)
            {
                continue;
            }

            var req = new Proxy.CRM.Models.Orders.CreateOrderReq
            {
                MagentoOrderId = x.OrderId
            };

            req.OrderInfo = _mapper.Map<Proxy.CRM.Models.Orders.Order>(x.Order);

            if (x.Order.Items != null)
            {
                req.Products = _mapper.Map<Proxy.CRM.Models.Orders.OrderItem[]>(x.Order.Items);

                i = 0;

                foreach (var y in x.Order.Items)
                {
                    _dbContext.OrderItem.Add(
                        new Domain.Entities.OrderItem
                        {
                            OrderId = y.OrderId,
                            OrderItemId = y.ItemId,
                            ProductId = y.ProductId,
                        }
                    );

                    i += 1;
                }

                if (i != 0)
                {
                    _dbContext.SaveChanges();
                }
            }

            var shippingAddress =
                x.Order.ExtensionAttributes.ShippingAssignments[0].Shipping.Address;

            req.OrderInfo.AccountEmail = shippingAddress.Email;
            req.OrderInfo.AccountPhone = shippingAddress.Telephone;

            req.OrderInfo.HasAccountPhone = !string.IsNullOrEmpty(req.OrderInfo.AccountPhone);

            // account_address
            if (shippingAddress.Street != null
                &&
                shippingAddress.Street.Length != 0)
            {
                req.OrderInfo.AccountAddress = shippingAddress.Street[0];
            }

            // account_email
            if (string.IsNullOrEmpty(req.OrderInfo.AccountEmail) == true)
            {
                req.OrderInfo.AccountEmail = x.Order.CustomerEmail;
            }

            req.OrderInfo.HasAccountEmail = !string.IsNullOrEmpty(req.OrderInfo.AccountEmail);

            // account_name
            sb.Clear();

            i = 0;

            if (string.IsNullOrEmpty(shippingAddress.Firstname) == false)
            {
                sb.Append(shippingAddress.Firstname);

                i += 1;
            }

            if (string.IsNullOrEmpty(shippingAddress.Lastname) == false)
            {
                if (i != 0)
                {
                    sb.Append(' ');
                }

                sb.Append(shippingAddress.Lastname);

                i += 1;
            }

            if (i == 0)
            {
                req.OrderInfo.AccountName = x.Order.CustomerFirstname;
            }
            else
            {
                req.OrderInfo.AccountName = sb.ToString();
            }

            lstReq.Add(req);
        }

        return lstReq.ToArray();
    }

    protected async Task<Proxy.CRM.Models.Orders.CreateOrderResp> CreateCrmOrder(Proxy.CRM.Models.Orders.CreateOrderReq req)
    {
        var resp = new Proxy.CRM.Models.Orders.CreateOrderResp
        {
            RequestData = req
        };

        try
        {
            resp.RequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(req);

            resp.OrderId = await _pxyGetfly.CreateOrderAsync(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when create order from Getfly");

            if (ex is Refit.ApiException)
            {
                var ex1 = (Refit.ApiException)ex;

                if (ex1.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var idx = ex1.Content.IndexOf("Duplicate entry", StringComparison.OrdinalIgnoreCase);

                    if (idx != -1)
                    {
                        resp.HasExisted = true;
                    }
                }
            }
        }

        resp.MagentoOrderId = req.MagentoOrderId;

        return resp;
    }

    protected byte[] GetBytes(string arg)
    {
        return System.Text.Encoding.UTF8.GetBytes(arg);
    }

    private async Task SyncToCrm(Proxy.CRM.Models.Orders.CreateOrderReq[] requests)
    {
        var lstTask = new List<Task<Proxy.CRM.Models.Orders.CreateOrderResp>>();

        //var i = 0;

        foreach (var x in requests)
        {
            //if (x.OrderInfo.HasMappedAccount == false)
            //{
            //    // khong the tao order o Getfly do khong co account
            //    continue;
            //}

            //i = 0;

            //if (x.Products != null)
            //{
            //    foreach (var y in x.Products)
            //    {
            //        if (string.IsNullOrEmpty(y.ProductCode) == true)
            //        {
            //            i += 1;

            //            break;
            //        }
            //    }
            //}

            //if (i != 0)
            //{
            //    // khong the tao order o Getfly do khong co product
            //    continue;
            //}

            lstTask.Add(CreateCrmOrder(x));

            if (x.Products != null)
            {
                foreach (var y in x.Products)
                {
                    var aOrderItem =
                        _dbContext.OrderItem.Where(z =>
                            z.OrderId == x.OrderInfo.Id
                            && z.OrderItemId == y.MagentoOrderItemId
                            && z.ProductId == y.MagentoProductId).ToArray();

                    if (aOrderItem != null)
                    {
                        foreach (var z in aOrderItem)
                        {
                            if (z != null && z.Id != 0)
                            {
                                z.GetflyProductCode = y.ProductCode;
                                z.GetflyProductId = y.ProductId;

                                _dbContext.OrderItem.Update(z);

                                _dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        var lstGetflyOrder = await Task.WhenAll(lstTask);

        if (lstGetflyOrder != null)
        {
            foreach (var x in lstGetflyOrder)
            {
                var enOrder =
                    _dbContext.Order.Where(y => y.OrderId == x.MagentoOrderId).FirstOrDefault();

                if (enOrder != null && enOrder.Id != 0)
                {
                    enOrder.GetflyRequestBody = GetBytes(x.RequestBody);

                    enOrder.SyncedOn = System.DateTime.Now;

                    if (x.HasExisted == true)
                    {
                        enOrder.Status = Domain.Constants.OrderStatus.HasSyncedBefore;
                    }
                    else if (x.OrderId == 0)
                    {
                        enOrder.Status = Domain.Constants.OrderStatus.Failed;
                    }
                    else
                    {
                        enOrder.Status = Domain.Constants.OrderStatus.Succeed;
                    }

                    _dbContext.Order.Update(enOrder);

                    _dbContext.SaveChanges();
                }
            }
        }
    }

    private async Task<Proxy.CRM.Models.Accounts.Account> GetCrmAccount(string keyword)
    {
        try
        {
            var resp = await _pxyGetfly.GetAccountListAsync(keyword);

            if (resp.Records == null || resp.Records.Length == 0)
            {
                return null;
            }

            return resp.Records[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when get account from Getfly");

            return null;
        }
    }

    private async Task<Proxy.CRM.Models.Accounts.CreateAccountResp> CreateCrmAccount(Proxy.CRM.Models.Accounts.Account account)
    {
        Proxy.CRM.Models.Accounts.CreateAccountResp resp;

        try
        {
            var req = new Proxy.CRM.Models.Accounts.CreateAccountReq
            {
                Account = account
            };

            req.Referrer = new Proxy.CRM.Models.Accounts.AccountReferrer
            {
                UtmCampaign = "Integration Api",
                UtmSource = "Integration Api"
            };

            resp = await _pxyGetfly.CreateAccountAsync(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when create account from Getfly");

            resp = new Proxy.CRM.Models.Accounts.CreateAccountResp();
        }

        resp.AccountEmail = account.Email;
        resp.AccountPhone = account.Phone;
        resp.MagentoCustomerId = account.CustomerId;

        return resp;
    }

    private async Task<Proxy.CRM.Models.Products.CreateProductResp> CreateCrmProduct(int productId)
    {
        Proxy.CRM.Models.Products.CreateProductResp resp;

        try
        {
            var req = new Proxy.CRM.Models.Products.CreateProductReq();

            req.ProductCode = productId.ToString();
            req.ProductName = req.ProductCode;

            resp = await _pxyGetfly.CreateProductAsync(req);

            resp.ProductCode = req.ProductCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when create product from Getfly");

            resp = new Proxy.CRM.Models.Products.CreateProductResp();
        }

        resp.MagenoProductId = productId;

        return resp;
    }

    private async Task<Proxy.CRM.Models.Products.Product> GetCrmProduct(int productId)
    {
        try
        {
            var resp = await _pxyGetfly.GetProductListAsync(productId.ToString());

            if (resp.Records == null || resp.Records.Length == 0)
            {
                return null;
            }

            var result = resp.Records[0];

            result.MagentoProductId = productId;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when get product from Getfly");

            return null;
        }
    }

    private async Task<Proxy.Magento.Models.Orders.GetOrderResp> GetMagentoOrder(int orderId)
    {
        var resp = new Proxy.Magento.Models.Orders.GetOrderResp
        {
            OrderId = orderId
        };

        try
        {
            resp.Order = await _pxyMagento.GetOrder(orderId);
            resp.ResponsePayload = Newtonsoft.Json.JsonConvert.SerializeObject(resp.Order);
            // resp.Order =
            //     Newtonsoft.Json.JsonConvert.DeserializeObject<Proxy.Magento.Models.Orders.Order>(resp.ResponsePayload, GetflyProxySetup.JsonSerializerSetting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when get order from MagentoApi");
        }

        return resp;
    }

    private DateTime? GetLastOrderTime()
    {
        var query = _dbContext.ScheduleLogging.Where(x => x.ApplicationName == ApplicationName.GET_FLY)
            .OrderByDescending(x => x.LastOrderTime)
            .Take(1);

        if (query.Any() == false)
        {
            return null;
        }

        return query.First().LastOrderTime;
    }

    #endregion
}
