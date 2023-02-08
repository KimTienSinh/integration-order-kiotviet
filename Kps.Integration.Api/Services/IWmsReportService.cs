using Kps.Integration.Api.Models.WmsReport;

namespace Kps.Integration.Api.Services;

public interface IWmsReportService
{
    public Task<(List<WmsReportViewModel>, int)> Search(SearchQuery searchQuery);
    public Task<(bool, string)> ReSync(int id);
}