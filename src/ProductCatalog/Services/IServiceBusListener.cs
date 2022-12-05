using ProductCatalog.Events;

namespace ProductCatalog.Services;

public interface IServiceBusService
{
    Task SendEventAsync<T>(T integrationEvent) where T : IntegrationEvent;
}