using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LogisticsAI.Api.Data;
using LogisticsAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAI.Api.Services;

public record AgentResponse(string Reply, string[] ToolCallsMade);

public interface IAgentService
{
    Task<AgentResponse> ChatAsync(Guid sessionId, string userMessage);
}

public class GroqAgentService(
    IHttpClientFactory httpFactory,
    AppDbContext db,
    IConfiguration config,
    ILogger<GroqAgentService> logger) : IAgentService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "llama-3.3-70b-versatile";
    private const string SystemPrompt =
        "You are a logistics analyst assistant. Use tools to query real shipment data " +
        "before answering. Be concise. For delays, always suggest an action.";
    private const int MaxIterations = 5;

    private string ApiKey => config["GROQ_API_KEY"] ?? "";

    // ── Tool definitions (OpenAI format) ──────────────────────────────────────

    private static readonly JsonNode Tools = JsonNode.Parse("""
        [
          {
            "type": "function",
            "function": {
              "name": "query_shipments",
              "description": "Query shipments from the database with optional filters. Returns a JSON array.",
              "parameters": {
                "type": "object",
                "properties": {
                  "status":          { "type": "string",  "description": "InTransit | Delayed | Critical | Delivered" },
                  "carrier":         { "type": "string",  "description": "FedEx | UPS | DHL | USPS" },
                  "min_delay_hours": { "type": "number",  "description": "Minimum delay in hours" },
                  "limit":           { "type": "integer", "description": "Max results (default 10)" }
                }
              }
            }
          },
          {
            "type": "function",
            "function": {
              "name": "get_shipment_detail",
              "description": "Get full details for one shipment including all events and latest RCA result.",
              "parameters": {
                "type": "object",
                "required": ["shipment_id"],
                "properties": {
                  "shipment_id": { "type": "string", "description": "UUID or tracking number of the shipment (e.g. 'UPS-2024-002')" }
                }
              }
            }
          },
          {
            "type": "function",
            "function": {
              "name": "run_rca",
              "description": "Run an AI root-cause analysis on a delayed/critical shipment and save the result.",
              "parameters": {
                "type": "object",
                "required": ["shipment_id"],
                "properties": {
                  "shipment_id": { "type": "string", "description": "UUID or tracking number of the shipment (e.g. 'UPS-2024-002')" }
                }
              }
            }
          }
        ]
        """)!;

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<AgentResponse> ChatAsync(Guid sessionId, string userMessage)
    {
        if (string.IsNullOrEmpty(ApiKey))
            return new AgentResponse("GROQ_API_KEY is not configured.", []);

        var session = await db.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null) return new AgentResponse("Session not found.", []);

        // Build messages — cap history at last 10 turns to stay within free-tier TPM
        var messages = new JsonArray();
        messages.Add(new JsonObject { ["role"] = "system", ["content"] = SystemPrompt });

        var history = session.Messages
            .Where(m => m.Role is "user" or "assistant")
            .TakeLast(10);

        foreach (var msg in history)
            messages.Add(new JsonObject { ["role"] = msg.Role, ["content"] = msg.Content });

        messages.Add(new JsonObject { ["role"] = "user", ["content"] = userMessage });

        // Persist user turn
        db.ChatMessages.Add(new ChatMessage
        {
            SessionId = sessionId,
            Role = "user",
            Content = userMessage,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        // ── Agentic loop ──────────────────────────────────────────────────────
        var http = httpFactory.CreateClient();
        string finalText = "Groq API call failed — check server logs.";
        int? tokensUsed = null;
        var toolCallsMade = new List<string>();

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            var response = await CallGroq(http, messages);
            if (response is null) break;

            var choice = response["choices"]?[0];
            var message = choice?["message"];
            if (message is null) break;

            tokensUsed = response["usage"]?["total_tokens"]?.GetValue<int>();
            var finishReason = choice?["finish_reason"]?.GetValue<string>();

            // ── Tool call branch ──────────────────────────────────────────────
            if (finishReason == "tool_calls")
            {
                var toolCalls = message["tool_calls"]?.AsArray();
                if (toolCalls is null) break;

                // Append assistant message with tool_calls
                messages.Add(new JsonObject
                {
                    ["role"] = "assistant",
                    ["content"] = (JsonNode?)null,
                    ["tool_calls"] = toolCalls.DeepClone()
                });

                // Execute each tool and append results
                foreach (var tc in toolCalls)
                {
                    var callId  = tc?["id"]?.GetValue<string>() ?? "";
                    var fnName  = tc?["function"]?["name"]?.GetValue<string>() ?? "";
                    toolCallsMade.Add(fnName);
                    var argsRaw = tc?["function"]?["arguments"]?.GetValue<string>() ?? "{}";

                    JsonObject args;
                    try { args = JsonNode.Parse(argsRaw)?.AsObject() ?? []; }
                    catch { args = []; }

                    var result = await ExecuteTool(fnName, args);

                    messages.Add(new JsonObject
                    {
                        ["role"]         = "tool",
                        ["tool_call_id"] = callId,
                        ["content"]      = result
                    });
                }
                continue;
            }

            // ── Text response branch ──────────────────────────────────────────
            var content = message["content"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(content))
            {
                finalText = content;
                break;
            }
        }

        // Persist assistant response
        db.ChatMessages.Add(new ChatMessage
        {
            SessionId  = sessionId,
            Role       = "assistant",
            Content    = finalText,
            TokensUsed = tokensUsed,
            CreatedAt  = DateTime.UtcNow
        });
        session.LastActiveAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new AgentResponse(finalText, [.. toolCallsMade]);
    }

    // ── Tool dispatch ─────────────────────────────────────────────────────────

    private Task<string> ExecuteTool(string name, JsonObject args) => name switch
    {
        "query_shipments"     => QueryShipments(args),
        "get_shipment_detail" => GetShipmentDetail(args),
        "run_rca"             => RunRca(args),
        _                     => Task.FromResult("""{"error":"Unknown tool"}""")
    };

    private async Task<string> QueryShipments(JsonObject args)
    {
        var query = db.Shipments.AsQueryable();

        if (args["status"]?.GetValue<string>() is string status &&
            Enum.TryParse<ShipmentStatus>(status, ignoreCase: true, out var statusEnum))
            query = query.Where(s => s.Status == statusEnum);

        if (args["carrier"]?.GetValue<string>() is string carrier)
            query = query.Where(s => s.Carrier.ToLower() == carrier.ToLower());

        if (args["min_delay_hours"] is JsonNode delayNode)
            query = query.Where(s => s.DelayHours >= (decimal)delayNode.GetValue<double>());

        int limit = args["limit"]?.GetValue<int>() ?? 10;

        var rows = await query
            .OrderByDescending(s => s.DelayHours)
            .Take(limit)
            .Select(s => new
            {
                s.Id, s.TrackingNumber, s.Carrier,
                s.OriginCity, s.DestinationCity,
                Status = s.Status.ToString(),
                s.DelayHours, s.ScheduledEta, s.WeightKg
            })
            .ToListAsync();

        return JsonSerializer.Serialize(rows, JsonOpts);
    }

    private async Task<Shipment?> ResolveShipment(string? raw, bool includeEvents = false, bool includeRca = false)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var query = db.Shipments.AsQueryable();
        if (includeEvents) query = query.Include(x => x.Events.OrderBy(e => e.OccurredAt));
        if (includeRca)    query = query.Include(x => x.RcaResults.OrderByDescending(r => r.AnalyzedAt));

        if (Guid.TryParse(raw, out var id))
            return await query.FirstOrDefaultAsync(x => x.Id == id);

        // Fall back to tracking number (case-insensitive)
        return await query.FirstOrDefaultAsync(x => x.TrackingNumber.ToLower() == raw.ToLower());
    }

    private async Task<string> GetShipmentDetail(JsonObject args)
    {
        var s = await ResolveShipment(args["shipment_id"]?.GetValue<string>(),
                    includeEvents: true, includeRca: true);

        if (s is null) return """{"error":"Shipment not found"}""";

        var detail = new
        {
            s.Id, s.TrackingNumber, s.Carrier,
            s.OriginCity, s.DestinationCity,
            Status = s.Status.ToString(),
            s.DelayHours, s.ScheduledEta, s.ActualEta, s.WeightKg,
            Events = s.Events.Select(e => new { e.EventType, e.Location, e.Description, e.OccurredAt }),
            LatestRca = s.RcaResults.FirstOrDefault() is RcaResult rca
                ? new { rca.RootCauseCategory, rca.AiSummary, rca.ConfidenceScore, rca.AnalyzedAt }
                : null
        };

        return JsonSerializer.Serialize(detail, JsonOpts);
    }

    private async Task<string> RunRca(JsonObject args)
    {
        var s = await ResolveShipment(args["shipment_id"]?.GetValue<string>(), includeEvents: true);
        if (s is null) return """{"error":"Shipment not found"}""";

        var eventLog = string.Join("\n", s.Events.Select(e =>
            $"[{e.OccurredAt:yyyy-MM-dd HH:mm}] {e.EventType} @ {e.Location}: {e.Description}"));

        var prompt =
            $"Shipment {s.TrackingNumber} ({s.Carrier}): {s.OriginCity} → {s.DestinationCity}. " +
            $"Delayed {s.DelayHours}h.\n\nEvents:\n{eventLog}\n\n" +
            "Classify root cause as exactly one of: WeatherDelay | MechanicalFailure | " +
            "TrafficDisruption | CustomsHold | CapacityIssue | UnknownDelay.\n" +
            """Respond ONLY with JSON: {"category":"...","summary":"2 sentences","confidence":0.0}""";

        var rcaMessages = new JsonArray
        {
            new JsonObject { ["role"] = "user", ["content"] = prompt }
        };

        var http = httpFactory.CreateClient();
        var rcaResponse = await CallGroq(http, rcaMessages, noTools: true);
        if (rcaResponse is null)
            return $"RCA failed for {s.TrackingNumber} (API rate limit — please try again in a moment).";

        var rcaText = rcaResponse["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
                      ?? """{"category":"UnknownDelay","summary":"Unable to determine root cause.","confidence":0.0}""";

        // Strip markdown code fences if present
        rcaText = rcaText.Trim();
        if (rcaText.StartsWith("```")) rcaText = rcaText.Split('\n', 2).Last().TrimEnd('`').Trim();

        string category = "UnknownDelay", summary = rcaText;
        decimal confidence = 0m;
        try
        {
            using var doc = JsonDocument.Parse(rcaText);
            var root = doc.RootElement;
            category   = root.GetProperty("category").GetString() ?? category;
            summary    = root.GetProperty("summary").GetString()  ?? summary;
            confidence = (decimal)root.GetProperty("confidence").GetDouble();
        }
        catch { /* keep defaults on malformed JSON */ }

        db.RcaResults.Add(new RcaResult
        {
            ShipmentId        = s.Id,
            RootCauseCategory = category,
            AiSummary         = summary,
            ConfidenceScore   = confidence,
            AnalyzedAt        = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return summary;
    }

    // ── HTTP helper ───────────────────────────────────────────────────────────

    private static readonly System.Text.RegularExpressions.Regex RetryAfterRegex =
        new(@"try again in (\d+(?:\.\d+)?)s", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    private async Task<JsonNode?> CallGroq(HttpClient http, JsonArray messages, bool noTools = false)
    {
        var body = new JsonObject
        {
            ["model"]       = Model,
            ["messages"]    = messages.DeepClone(),
            ["temperature"] = 0.3,
            ["max_tokens"]  = 1024        // cap output to save TPM headroom
        };

        if (!noTools)
        {
            body["tools"]       = Tools.DeepClone();
            body["tool_choice"] = "auto";
        }

        var requestJson = body.ToJsonString();
        logger.LogDebug("Groq request: {Request}", requestJson);

        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {ApiKey}");

            var resp = await http.SendAsync(request);
            var responseText = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                logger.LogDebug("Groq response: {Response}", responseText);
                return JsonNode.Parse(responseText);
            }

            // Always log 429 as an error so it's visible in the terminal
            if ((int)resp.StatusCode == 429)
            {
                double waitSeconds = 15;
                var match = RetryAfterRegex.Match(responseText);
                if (match.Success && double.TryParse(match.Groups[1].Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    waitSeconds = Math.Min(parsed + 1, 20); // cap wait at 20s
                }

                logger.LogError("Groq 429 rate-limit (attempt {Attempt}/{Max}) — waiting {Wait:F1}s. Body: {Body}",
                    attempt, maxAttempts, waitSeconds, responseText);

                if (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
                    continue;
                }

                return null;
            }

            logger.LogError("Groq API error {Status}: {Body}", (int)resp.StatusCode, responseText);
            return null;
        }

        return null;
    }
}
