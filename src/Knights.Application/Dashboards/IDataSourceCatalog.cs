using Knights.Application.Dashboards.Models;

namespace Knights.Application.Dashboards;

public interface IDataSourceCatalog
{
    IReadOnlyCollection<DataSourceDescriptorResponse> GetAll();
    DataSourceDescriptorResponse GetRequired(string key);
}
