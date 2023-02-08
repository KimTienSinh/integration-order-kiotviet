using Newtonsoft.Json;

namespace Kps.Integration.Proxy.CRM.Models.Accounts;

public sealed class Account
{
    public int AccountId { get; set; }

    [JsonIgnore]
    public int CustomerId { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AccountCode { get; set; }

    public string AccountName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string PhoneOffice { get; set; }
}
