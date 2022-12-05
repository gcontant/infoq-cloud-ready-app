using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ProductCatalog.Events;

namespace ProductCatalog.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusSender _serviceBusSender;

    public ServiceBusService(
        ServiceBusClient serviceBusClient,
        IConfiguration configuration)
    {
        _serviceBusSender = serviceBusClient.CreateSender(configuration["QueueName"]);
    }

    public async Task SendEventAsync<T>(T integrationEvent)
        where T : IntegrationEvent
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string messagePayload = JsonSerializer.Serialize(integrationEvent, options);
        var message = new ServiceBusMessage(messagePayload);

        await _serviceBusSender.SendMessageAsync(message);
    }
}