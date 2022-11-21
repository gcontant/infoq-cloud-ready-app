using System.Text.Json.Serialization;
namespace ProductCatalog.Models;

public class UpdateProductRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("owner")]
    public string Owner { get; set; }
}