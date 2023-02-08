namespace Kps.Integration.Proxy.Magento.Models.Products;

public class GetProductResponse
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public List<CustomAttribute> CustomAttributes { get; set; }

    public ShippingTime GetShippingTime()
    {
        var shippingTime = new ShippingTime()
        {
            From = 0,
            To = 1,
            Type = "Giờ"
        };
        foreach (var attribute in CustomAttributes)
        {
            if (attribute.AttributeCode == "time_shipping_from")
            {
                if(int.TryParse(attribute.Value, out int result))
                {
                    shippingTime.From = result;
                }
            }

            if (attribute.AttributeCode == "time_shipping_to")
            {
                if(int.TryParse(attribute.Value, out int result))
                {
                    shippingTime.To = result;
                }
            }

            if (attribute.AttributeCode == "time_shipping_type")
            {
                shippingTime.Type = (string) attribute.Value;
            }
        }

        return shippingTime;
    }
}

public class CustomAttribute
{
    public string AttributeCode { get; set; }
    public dynamic Value { get; set; }
}

public class ShippingTime
{
    public string Sku { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public string Type { get; set; }
}