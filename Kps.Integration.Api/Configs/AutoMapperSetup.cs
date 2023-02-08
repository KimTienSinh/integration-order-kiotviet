using AutoMapper;
using Kps.Integration.Api.Models.Orders;

namespace Kps.Integration.Api.Configs;

public static class AutoMapperSetup
{
    public static IServiceCollection UseAutoMapper(this IServiceCollection services)
    {
        services.AddSingleton(t =>
            {
                var mappingConfig =
                    new AutoMapper.MapperConfiguration(AutoMapperSetup.Register);

                return mappingConfig.CreateMapper();
            }
        );

        return services;
    }

    private static void Register(IMapperConfigurationExpression config)
    {
        config
            .CreateMap<Domain.Entities.Order, Models.Reports.SearchRespItem>()
            .ForMember(
                des => des.CanReSync,
                opt => opt.MapFrom(src =>
                    src.Status.Equals(Domain.Constants.OrderStatus.Succeed) == false
                    && src.Status.Equals(Domain.Constants.OrderStatus.HasSyncedBefore) == false));

        config
            .CreateMap<Proxy.CRM.Models.Orders.Order, Proxy.CRM.Models.Accounts.Account>()
            .ForMember(des => des.AccountCode, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(des => des.Email, opt => opt.MapFrom(src => src.AccountEmail))
            .ForMember(des => des.Phone, opt => opt.MapFrom(src => src.AccountPhone))
            .ForMember(des => des.PhoneOffice, opt => opt.MapFrom(src => src.AccountPhone));

        config
            .CreateMap<Proxy.Magento.Models.Orders.OrderItem, Proxy.CRM.Models.Orders.OrderItem>()
            .ForMember(des => des.MagentoOrderItemId, opt => opt.MapFrom(src => src.ItemId))
            .ForMember(des => des.MagentoProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(des => des.Quantity, opt => opt.MapFrom(src => src.QtyOrdered))
            .ForMember(des => des.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(des => des.Vat, opt => opt.MapFrom(src => src.TaxPercent));

        config
            .CreateMap<Proxy.Magento.Models.Orders.Order, Proxy.CRM.Models.Orders.Order>()
            //.ForMember(des => des.AccountEmail, opt => opt.MapFrom(src => src.CustomerEmail))
            //.ForMember(des => des.AccountName, opt => opt.MapFrom(src => src.CustomerFirstname))
            .ForMember(des => des.Amount, opt => opt.MapFrom(src => src.GrandTotal))
            .ForMember(des => des.Id, opt => opt.MapFrom(src => src.EntityId))
            .ForMember(des => des.OrderCode, opt => opt.MapFrom(src => src.EntityId.ToString()))
            .ForMember(des => des.OrderDate, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd/MM/yyyy")))
            .ForMember(des => des.SourceName, opt => opt.MapFrom(src => "Integration Api"))
            .ForMember(des => des.TransportAmount, opt => opt.MapFrom(src => src.ShippingAmount))
            .ForMember(des => des.Vat, opt => opt.MapFrom(src => src.TaxAmount));

        config
            .CreateMap<Proxy.Magento.Models.Orders.Order, Proxy.WMS.Models.Orders.Order>()
            .ForMember(des => des.DO_No, opt => opt.MapFrom(src => src.IncrementId))
            .ForMember(des => des.DO_Date, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd/MM/yyyy")))
            .ForMember(des => des.Ref_No, opt => opt.MapFrom(src => ""))
            .ForMember(des => des.API_Ref_ID, opt => opt.MapFrom(src => src.EntityId))
            .ForMember(des => des.Warehouse_Code, opt => opt.MapFrom(src => "KPS"))
            .ForMember(des => des.Owner_Code, opt => opt.MapFrom(src => "KPS"))
            .ForMember(des => des.Type_ID, opt => opt.MapFrom(src => 4))
            .ForMember(des => des.Receiver, opt => opt.MapFrom(src => GetReceiverName(src)))
            .ForMember(des => des.Address_Phone, opt => opt.MapFrom(src =>
                src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.Telephone))
            .ForMember(des => des.Address_Email, opt => opt.MapFrom(src =>
                src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.Email))
            .ForMember(des => des.Address_Full, opt => opt.MapFrom(src => GetAddress(src)))
            .ForMember(des => des.Address_City, opt => opt.MapFrom(src =>
                src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.City))
            .ForMember(des => des.Address_District, opt => opt.MapFrom(src =>
                src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.District ?? ""))
            .ForMember(des => des.Address_Ward,
                opt => opt.MapFrom(src => src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.Ward ?? ""))
            // .ForMember(des => des.Customer_Code, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(des => des.Customer_Name, opt => opt.MapFrom(src => GetCustomerName(src)))
            .ForMember(des => des.Customer_Phone,
                opt => opt.MapFrom(src => src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.Telephone))
            .ForMember(des => des.Customer_Email, opt => opt.MapFrom(src => src.CustomerEmail))
            .ForMember(des => des.Customer_Address_Full, opt => opt.MapFrom(src => GetAddress(src)))
            .ForMember(des => des.Customer_City,
                opt => opt.MapFrom(src => src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.City))
            .ForMember(des => des.Customer_District,
                opt => opt.MapFrom(
                    src => src.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.District ?? ""))
            .ForMember(des => des.Tri_Gia_Xuat_Final, opt => opt.MapFrom(src => src.GrandTotal))
            .ForMember(des => des.So_Tien_COD, opt => opt.MapFrom(src => GetCashOnDeliveryAmount(src)))
            .ForMember(des => des.Customer_Tax_Code, opt => opt.MapFrom(src => ""))
            .ForMember(des => des.Customer_Tax_Name, opt => opt.MapFrom(src => ""))
            .ForMember(des => des.Customer_Tax_Address, opt => opt.MapFrom(src => ""))
            .ForMember(des => des.Customer_Tax_Email, opt => opt.MapFrom(src => src.CustomerEmail))
            .ForMember(des => des.Note, opt => opt.MapFrom(src => GetNotes(src)))
            .ForMember(des => des.Delivery_Fee, opt => opt.MapFrom(src => src.ShippingAmount))
            ;

        config
            .CreateMap<Proxy.Magento.Models.Orders.OrderItem, Proxy.WMS.Models.Orders.OrderItem>()
            .ForMember(des => des.Item_Code, opt => opt.MapFrom(src => src.Sku))
            .ForMember(des => des.API_Ref_ID, opt => opt.MapFrom(src => src.ItemId))
            .ForMember(des => des.Qty, opt => opt.MapFrom(src => src.QtyOrdered))
            .ForMember(des => des.Unit_Price, opt => opt.MapFrom(src => 0))
            .ForMember(des => des.Total_Value, opt => opt.MapFrom(src => 0))
            .ForMember(des => des.Warehouse_Code, opt => opt.MapFrom(src => "KPS"))
            .ForMember(des => des.Discount_Amount, opt => opt.MapFrom(src => src.DiscountAmount))
            .ForMember(des => des.So_Ngay_Dat_Truoc, opt => opt.MapFrom(src => 0))
            .ForMember(des => des.Notes, opt => opt.MapFrom(src => ""))
            .ForMember(des => des.Line_Item, opt => opt.MapFrom(src => ""))
            ;

        config
            .CreateMap<OrderEntityNotification, Proxy.Magento.Models.Orders.UpdateOrderEntity>()
            .ForMember(des => des.EntityId, opt => opt.MapFrom(src => src.Entity_Id))
            ;
    }

    private static string GetAddress(Proxy.Magento.Models.Orders.Order order)
    {
        return string.Join(",",
            order.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.Street[0],
            order.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.District,
            order.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.City
        );
    }

    private static double GetCashOnDeliveryAmount(Proxy.Magento.Models.Orders.Order order)
    {
        return order.Payment.Method == "cashondelivery" ? (double) order.GrandTotal : 0;
    }

    private static string GetCustomerName(Proxy.Magento.Models.Orders.Order order)
    {
        if (string.IsNullOrEmpty(order.CustomerFirstname))
        {
            return "Guest";
        }

        return $"{order.CustomerFirstname} {order.CustomerLastname}".Trim();
    }

    private static string GetReceiverName(Proxy.Magento.Models.Orders.Order order)
    {
        var name = $"{order.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.Firstname} " +
                   $"{order.ExtensionAttributes.ShippingAssignments[0].Shipping.Address.Lastname}";
        if (string.IsNullOrEmpty(name))
        {
            return GetCustomerName(order);
        }

        return name;
    }

    private static string GetNotes(Proxy.Magento.Models.Orders.Order order)
    {
        if (order.StatusHistories == null || order.StatusHistories.Count == 0)
        {
            return "";
        }

        var systemComments = new string[]
        {
            "Order Placed by",
            "Order were confirmed by",
            "Ordered amount of",
            "failed to sync to WMS"
        };

        var notes = order.StatusHistories
            .Where(t => !systemComments.Any(s=> t.Comment.Contains(s)))
            .OrderBy(t => t.CreatedAt)
            .Select(t => $"[{t.GetCreatedAtLocalDateTime().ToString("yyyy-MM-dd hh:mm:ss")}] {t.Comment}").ToList();

        return string.Join("<br/>", notes);
    }
}