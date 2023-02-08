using Kps.Integration.Proxy.Magento.Models.Products;

namespace Kps.Integration.Proxy.WMS.Models.Orders;

public class CreateOrderRequest : WmsRequestBase
{
    public Order Data { get; set; }
}

public class Order
{
    public string DO_No { get; set; }
    public string DO_Date { get; set; } //format dd/MM/yyyy
    public string Ref_No { get; set; }

    public string Owner_Code { get; set; } = "KPS";
    public int Type_ID { get; set; } = 4; // Xuất B2C

    public string Receiver { get; set; }
    public string Address_Phone { get; set; }
    public string Address_Email { get; set; }
    public string Address_Full { get; set; }
    public string Address_City { get; set; }
    public string Address_District { get; set; }
    public string Address_Ward { get; set; }
    public string Customer_Name { get; set; }
    public string Customer_Phone { get; set; }
    public string Customer_Email { get; set; }
    public string Customer_Address_Full { get; set; }
    public string Customer_City { get; set; }
    public string Customer_District { get; set; }
    public string Customer_Tax_Code { get; set; }
    public string Customer_Tax_Name { get; set; }
    public string Customer_Tax_Email { get; set; }
    public string Customer_Tax_Address { get; set; }
    public string Customer_Code { get; set; }

    public double Tri_Gia_Xuat_Final { get; set; }
    public double So_Tien_COD { get; set; }
    public double Delivery_Fee { get; set; }
    public double VAT_Amount { get; set; }
    public string Ngay_Gio_Du_Kien_Giao { get; set; }
    public string Note { get; set; }

    public string API_Ref_ID { get; set; }
    public string Warehouse_Code { get; set; } = "KPS";
    public List<OrderItem> Transaction_Detail { get; set; }

    public void SetEstimationShippingTime(List<ShippingTime> shippingTimes, DateTime createAt)
    {
        var calculatedShippingTime = CalculateShippingTime(shippingTimes);

        DateTime estimatedShippingTime;
        if (calculatedShippingTime.Type == "Giờ" || calculatedShippingTime.Type == "Hour")
        {
            estimatedShippingTime = createAt.AddMinutes(calculatedShippingTime.From * 60 * 60);
        }
        else
        {
            estimatedShippingTime = createAt.AddMinutes(calculatedShippingTime.From * 60 * 60 * 24);
        }


        this.Ngay_Gio_Du_Kien_Giao = estimatedShippingTime.ToString("dd/MM/yyyy hh:mm");
        var timeNote = $"Time shipping: {calculatedShippingTime.From} - {calculatedShippingTime.To} " +
                       $"{calculatedShippingTime.Type}";

        this.Note = string.IsNullOrEmpty(this.Note) ? timeNote : $"{this.Note}<br> {timeNote}";

        foreach (var shippingTime in shippingTimes)
        {
            var orderItem = Transaction_Detail.Where(t => t.Item_Code == shippingTime.Sku).FirstOrDefault();
            if (orderItem != null)
            {
                orderItem.SetDeliveryHour(shippingTime);
            }
        }
    }

    private ShippingTime CalculateShippingTime(List<ShippingTime> shippingTimes)
    {
        var calculatedShippingTime = new ShippingTime()
        {
            From = 0,
            To = 1,
            Type = "Giờ"
        };

        var hourText = new[] {"Giờ", "Hour"};
        var dateText = new[] {"Ngày", "Date"};

        var hasDateType = shippingTimes.Any(t => dateText.Contains(t.Type));

        if (hasDateType)
        {
            calculatedShippingTime.From = shippingTimes.Where(t => dateText.Contains(t.Type)).Select(t=> t.From).Max();
            calculatedShippingTime.To = shippingTimes.Where(t => dateText.Contains(t.Type)).Select(t=> t.To).Max();
            calculatedShippingTime.Type = "Ngày";
        }
        else
        {
            calculatedShippingTime.From = shippingTimes.Select(t=> t.From).Max();
            calculatedShippingTime.To = shippingTimes.Select(t=> t.To).Max();
        }

        calculatedShippingTime.To = Math.Max(calculatedShippingTime.From, calculatedShippingTime.To);

        if (calculatedShippingTime.From == 0 && calculatedShippingTime.To == 0 ||
            (hourText.Contains(calculatedShippingTime.Type) && calculatedShippingTime.To < 23))
        {
            calculatedShippingTime.From = 1;
            calculatedShippingTime.To = 1;
            calculatedShippingTime.Type = "Ngày";
        }

        return calculatedShippingTime;
    }
}

public class OrderItem
{
    public int API_Ref_ID;
    public string Item_Code { get; set; }
    public string Item_Name { get; set; }
    public double Qty { get; set; }
    public double Unit_Price { get; set; }
    public double Total_Value { get; set; }

    public string Warehouse_Code { get; set; }
    public double Discount_Amount { get; set; }
    public double So_Ngay_Dat_Truoc { get; set; }
    public string Notes { get; set; }
    public string Line_Item { get; set; }
    public int So_Gio_YC_Giao_From { get; set; } = 0;
    public int So_Gio_YC_Giao_To { get; set; } = 0;

    public void SetDeliveryHour(ShippingTime shippingTime)
    {
        const int MORE_THAN_5_DATE = 9999;
        var dateText = new[] {"Ngày", "Date"};
        var shippingTimeFrom = shippingTime.From;
        var shippingTimeTo = shippingTime.To;
        
        if (!(shippingTimeFrom == 0 && shippingTimeTo == 0))
        {
            if (shippingTimeTo == 0 || shippingTimeTo == MORE_THAN_5_DATE)
            {
                shippingTimeTo = MORE_THAN_5_DATE;
            }

            if (dateText.Contains(shippingTime.Type))
            {
                shippingTimeFrom = shippingTimeFrom * 24;
                shippingTimeTo = shippingTimeTo != MORE_THAN_5_DATE ? shippingTimeTo * 24 : shippingTimeTo;
            }

            this.So_Gio_YC_Giao_From = shippingTimeFrom;
            this.So_Gio_YC_Giao_To = shippingTimeTo;
        }
    }
}