namespace MiniHelpdesk.Models;

public class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TicketStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}