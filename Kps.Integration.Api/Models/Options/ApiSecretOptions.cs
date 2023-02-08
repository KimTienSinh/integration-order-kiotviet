namespace Kps.Integration.Api.Models.Options;

public class ApiSecretOptions
{
    public List<ApiSecret> ApiSecrets { get; set; }
}

public class ApiSecret
{
    public string Name { get; set; }
    public string Key { get; set; }
}