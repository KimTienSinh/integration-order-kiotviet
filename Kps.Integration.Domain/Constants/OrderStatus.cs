namespace Kps.Integration.Domain.Constants;

public sealed class OrderStatus
{
    public const string
        Init = "INIT",
        Succeed = "SUCCEED",
        Failed = "FAILED",
        FailedByMagentoApi = "FAILED_BY_MAGENTO_API",
        InvalidDataFromMagentoApi = "INVALID_DATA_FROM_MAGENTO_API",
        HasSyncedBefore = "HAS_SYNCED_BEFORE";
}
