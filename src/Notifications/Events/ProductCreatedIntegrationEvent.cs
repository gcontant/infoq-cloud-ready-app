using System.Text.Json.Serialization;

namespace Notifications.Events;

public class ProductCreatedIntegrationEvent
        : IntegrationEvent
    {
        [JsonPropertyName("id")]
        public Guid Id { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("price")]
        public decimal Price { get; }

        [JsonPropertyName("owner")]
        public string Owner { get; }

        [JsonConstructor]
        public ProductCreatedIntegrationEvent(
            Guid id,
            string name,
            decimal price,
            string owner)
        {
            Id = id;
            Name = name;
            Price = price;
            Owner = owner;
        }
    }

    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            MessageId = Guid.NewGuid();
        }

        [JsonPropertyName("messageId")]
        public Guid MessageId { get; }
    }