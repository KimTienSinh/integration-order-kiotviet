using Newtonsoft.Json;

namespace Kps.Integration.Proxy.CRM.Models.Accounts;

public sealed class CreateAccountReq
{
    public Account Account { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public AccountContact[] Contacts { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public AccountReferrer Referrer { get; set; }
}

public sealed class CreateAccountResp
{
    public int AccountId { get; set; }

    public int Code { get; set; }

    [JsonIgnore]
    public int MagentoCustomerId { get; set; }

    [JsonIgnore]
    public string AccountCode { get; set; }

    [JsonIgnore]
    public string AccountEmail { get; set; }

    [JsonIgnore]
    public string AccountPhone { get; set; }

    public string Message { get; set; }
}

public sealed class AccountContact
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ContactId { get; set; }

    public string Email { get; set; }

    public string FirstName { get; set; }

    public string PhoneMobile { get; set; }
}

public sealed class AccountReferrer
{
    public string UtmCampaign { get; set; }

    public string UtmSource { get; set; }
}
