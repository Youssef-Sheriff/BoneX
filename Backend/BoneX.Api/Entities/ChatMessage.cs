namespace BoneX.Api.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty; // FK to ApplicationUser
    public string ReceiverId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
