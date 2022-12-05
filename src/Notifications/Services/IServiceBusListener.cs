namespace Notifications.Services;

public interface IServiceBusListener
{
    Task RegisterAsync();
}