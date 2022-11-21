using System.Text.Json.Serialization;

namespace ProductCatalog.Models.DTO;

public class ApiResponse
{
    public ApiResponse(string message)
    {
        Message = message;
    }

    [JsonPropertyName("message")]
    public string Message { get; }
}
