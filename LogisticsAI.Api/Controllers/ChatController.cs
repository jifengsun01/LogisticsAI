using LogisticsAI.Api.Data;
using LogisticsAI.Api.Models;
using LogisticsAI.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IAgentService agent, AppDbContext db) : ControllerBase
{
    public record ChatRequest(string SessionId, string Message);
    public record MessageDto(Guid Id, string Role, string Content, int? TokensUsed, DateTime CreatedAt);

    // POST /api/chat/messages
    [HttpPost("messages")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty.");

        if (!Guid.TryParse(request.SessionId, out var sessionId))
            return BadRequest("Invalid sessionId.");

        var result = await agent.ChatAsync(sessionId, request.Message);
        return Ok(new { reply = result.Reply, toolCallsMade = result.ToolCallsMade });
    }

    // GET /api/chat/sessions
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions() =>
        Ok(await db.ChatSessions.OrderByDescending(s => s.LastActiveAt).ToListAsync());

    // POST /api/chat/sessions
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] string name)
    {
        var session = new ChatSession { SessionName = name };
        db.ChatSessions.Add(session);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMessages), new { sessionId = session.Id }, session);
    }

    // GET /api/chat/sessions/{sessionId}/messages
    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid sessionId)
    {
        var messages = await db.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(m.Id, m.Role, m.Content, m.TokensUsed, m.CreatedAt))
            .ToListAsync();
        return Ok(messages);
    }
}
