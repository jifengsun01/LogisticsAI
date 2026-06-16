using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LogisticsAI.Api.Services;

public interface IAiInsightService
{
    Task<string> GetShipmentInsightAsync(string origin, string destination, decimal weightKg);
}

public class GroqInsightService(IHttpClientFactory httpClientFactory, IConfiguration config) : IAiInsightService
{
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model  = "llama-3.1-8b-instant";

    public async Task<string> GetShipmentInsightAsync(string origin, string destination, decimal weightKg)
    {
        var apiKey = config["GROQ_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))
            return "AI insight unavailable: GROQ_API_KEY not configured.";

        var prompt =
            $"You are a logistics AI assistant. Provide a brief 2-sentence insight for a shipment " +
            $"from {origin} to {destination} weighing {weightKg}kg. " +
            $"Include estimated transit risk and one optimization tip.";

        var body = new JsonObject
        {
            ["model"]    = Model,
            ["messages"] = new JsonArray(
                new JsonObject { ["role"] = "user", ["content"] = prompt }
            ),
            ["temperature"] = 0.3
        };

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var client   = httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return "AI insight unavailable: upstream error.";

        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync());
        return json?["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
               ?? "No insight returned.";
    }
}
