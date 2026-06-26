namespace MiniHelpdesk.Models;

public class TicketComment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public string Author { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}