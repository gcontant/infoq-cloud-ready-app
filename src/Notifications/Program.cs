using Microsoft.Extensions.Azure;
using Notifications.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(clients =>
    {
        var connectionString = builder.Configuration.GetConnectionString("ServiceBus");
        clients.AddServiceBusClient(connectionString);
    });

builder.Services.AddSingleton<IServiceBusListener, ServiceBusListener>();
var app = builder.Build();

var bus = app.Services.GetService<IServiceBusListener>();
bus!.RegisterAsync();
