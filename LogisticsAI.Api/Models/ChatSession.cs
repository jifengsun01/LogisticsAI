namespace LogisticsAI.Api.Models;

public class ChatSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SessionName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
