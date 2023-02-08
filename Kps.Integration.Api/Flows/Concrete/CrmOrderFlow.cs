using AutoMapper;
using Kps.Integration.Api.Infra;
using Kps.Integration.Api.Services;
using Kps.Integration.Proxy.CRM;
using Kps.Integration.Proxy.Magento;
using Microsoft.AspNetCore.Identity;

namespace Kps.Integration.Api.Flows.Concrete;

public class CrmOrderFlow :
    Kps.Integration.Api.Flows.Concrete.SyncMagentoOrderToCrmFlow,
    Kps.Integration.Api.Flows.ICrmOrderFlow
{
    public CrmOrderFlow(
        IGetflyProxy pxyGetfly,
        ILogger<SyncMagentoOrderToCrmFlow> logger,
        IMagentoProxy pxyMagento,
        IMagentoService svcMagento,
        IMapper mapper,
        KpsIntegrationContext dbContext)
        : base(pxyGetfly, logger, pxyMagento, svcMagento, mapper, dbContext)
    {
        
    }

    #region Methods 

    public async Task<bool> ReSync(IdentityUser user, int id)
    {
        var enOrder = _dbContext.Order.Where(x => x.Id == id).FirstOrDefault();

        if (enOrder == null || enOrder.Id == 0)
        {
            return false;
        }

        switch (enOrder.Status)
        {
            case Domain.Constants.OrderStatus.Succeed:
            case Domain.Constants.OrderStatus.HasSyncedBefore:
                return true;
            default:
                break;
        }

        var aOrderItem =
            _dbContext.OrderItem.Where(x => x.OrderId == enOrder.OrderId).ToArray();

        if (aOrderItem != null)
        {
            var i = 0;

            foreach (var x in aOrderItem)
            {
                _dbContext.OrderItem.Remove(x);

                i++;
            }

            if (i != 0)
            {
                _dbContext.SaveChanges();
            }
        }

        var aGetflyOrder = await PrepareCrmOrder(enOrder);

        if (aGetflyOrder == null || aGetflyOrder.Length == 0)
        {
            _logger.LogInformation("Failed (not found any order from MagentoApi)");

            return false;
        }

        await PrepareCrmAccount(aGetflyOrder);

        await PrepareCrmProduct(aGetflyOrder);

        return await ReSyncToCrm(user.Id, aGetflyOrder[0]);
    }

    public Models.Reports.SearchResp Search(Models.Reports.SearchReq req)
    {
        var resp = new Models.Reports.SearchResp();

        const int iMaxRequestCount = 20;

        var iRequestCount = 0;
        var iRequestPage = 0;

        var query = _dbContext.Order.AsQueryable();

        if (req == null)
        {
            iRequestCount = iMaxRequestCount;

            iRequestPage = 1;
        }
        else
        {
            var str = this.VerifySearchInput(ref query, req, iMaxRequestCount);

            if (str == null)
            {
                if (req.RequestPage < 1)
                {
                    str = "requestPage";
                }
                else
                {
                    iRequestPage = req.RequestPage;
                }

                if (req.RequestCount < 1 || req.RequestCount > iMaxRequestCount)
                {
                    str = "requestCount";
                }
                else
                {
                    iRequestCount = req.RequestCount;
                }
            }

            if (str != null)
            {
                resp.Code = 400;
                resp.Message = $"Bad request ({str})";

                return resp;
            }
        }

        resp.Code = 0;
        resp.Message = "Success";

        resp.Data = new Models.Reports.SearchRespData();
        resp.Data.Total = query.Count();

        if (resp.Data.Total == 0)
        {
            return resp;
        }

        query = query.OrderByDescending(x => x.Id);

        if (iRequestPage > 1)
        {
            query = query.Skip((iRequestPage - 1) * iRequestCount);
        }

        query = query.Take(iRequestCount);

        var arr = query.ToArray();

        resp.Data.Records = _mapper.Map<Models.Reports.SearchRespItem[]>(arr);

        return resp;
    }

    #endregion

    #region Manipulations 

    private async Task<bool> ReSyncToCrm(string retryBy, Proxy.CRM.Models.Orders.CreateOrderReq request)
    {
        if (request.Products != null)
        {
            foreach (var y in request.Products)
            {
                var aOrderItem =
                    _dbContext.OrderItem.Where(z =>
                        z.OrderId == request.OrderInfo.Id
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

        var bIsSucceed = false;

        var getflyOrder = await CreateCrmOrder(request);

        if (getflyOrder != null)
        {
            var enOrder =
                _dbContext.Order.Where(y => y.OrderId == getflyOrder.MagentoOrderId).FirstOrDefault();

            if (enOrder != null && enOrder.Id != 0)
            {
                enOrder.GetflyRequestBody = GetBytes(getflyOrder.RequestBody);

                enOrder.RetryCount += 1;
                enOrder.RetriedBy = retryBy;

                enOrder.SyncedOn = System.DateTime.Now;

                if (getflyOrder.HasExisted == true)
                {
                    enOrder.Status = Domain.Constants.OrderStatus.HasSyncedBefore;

                    bIsSucceed = true;
                }
                else if (getflyOrder.OrderId == 0)
                {
                    enOrder.Status = Domain.Constants.OrderStatus.Failed;
                }
                else
                {
                    enOrder.Status = Domain.Constants.OrderStatus.Succeed;

                    bIsSucceed = true;
                }

                _dbContext.Order.Update(enOrder);

                _dbContext.SaveChanges();
            }
        }

        return bIsSucceed;
    }

    private bool TryParseDateTime(string arg, out DateTime result, string format = "dd/MM/yyyy HH:mm:ss")
    {
        return DateTime.TryParseExact(
            result: out result,
            s: arg,
            format: format,
            style: System.Globalization.DateTimeStyles.None,
            provider: System.Globalization.CultureInfo.InvariantCulture);
    }

    private string VerifySearchInput(
        ref IQueryable<Domain.Entities.Order> query, Models.Reports.SearchReq req, int maxRequestCount)
    {
        if (string.IsNullOrEmpty(req.Status) == true)
        {
            //return "status";
        }
        else
        {
            switch (req.Status)
            {
                case Domain.Constants.OrderStatus.Failed:
                case Domain.Constants.OrderStatus.FailedByMagentoApi:
                case Domain.Constants.OrderStatus.Init:
                case Domain.Constants.OrderStatus.InvalidDataFromMagentoApi:
                case Domain.Constants.OrderStatus.Succeed:
                    break;
                default:
                    return "status";
            }

            query = query.Where(x => x.Status.Equals(req.Status));
        }

        if (string.IsNullOrEmpty(req.FromDate) == true)
        {
            //return "fromDate";
        }
        else
        {
            DateTime val;

            var str = string.Join("", req.FromDate, " 00:00:00");

            var bIsSucceed = this.TryParseDateTime(str, out val);

            if (bIsSucceed == false)
            {
                return "fromDate";
            }

            query = query.Where(x => x.OrderCreatedOn != null && DateTime.Compare(x.OrderCreatedOn.Value, val) >= 0);
        }

        if (string.IsNullOrEmpty(req.ToDate) == true)
        {
            //return "toDate";
        }
        else
        {
            DateTime val;

            var str = string.Join("", req.ToDate, " 23:59:59");

            var bIsSucceed = this.TryParseDateTime(str, out val);

            if (bIsSucceed == false)
            {
                return "toDate";
            }

            query = query.Where(x => x.OrderCreatedOn != null && DateTime.Compare(x.OrderCreatedOn.Value, val) < 0);
        }

        return null;
    }

    #endregion
}
