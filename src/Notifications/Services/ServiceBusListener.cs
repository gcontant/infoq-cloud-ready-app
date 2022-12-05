using Azure.Messaging.ServiceBus;
using Notifications.Events;

namespace Notifications.Services;

public class ServiceBusListener : IServiceBusListener
{
    private readonly ServiceBusProcessor _serviceBusSender;
    private readonly ILogger<ServiceBusListener> _logger;

    public ServiceBusListener(
        ServiceBusClient serviceBusClient,
        IConfiguration configuration,
        ILogger<ServiceBusListener> logger)
    {
        var serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
        };

        _serviceBusSender = serviceBusClient.CreateProcessor(
            configuration["QueueName"],
            serviceBusProcessorOptions);

        _logger = logger;
    }

    public async Task RegisterAsync()
    {
        _serviceBusSender.ProcessMessageAsync += ProcessMessagesAsync;
        _serviceBusSender.ProcessErrorAsync += ProcessErrorAsync;

        await _serviceBusSender.StartProcessingAsync();
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Message handler encountered an exception");
        return Task.CompletedTask;
    }

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        ProductCreatedIntegrationEvent myPayload = args.Message.Body.ToObjectFromJson<ProductCreatedIntegrationEvent>();

        if (!EmailHelper.IsEmail(myPayload.Owner))
        {
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        try
        {
            // send the email as you prefer
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error sending notification");
        }
    }
}