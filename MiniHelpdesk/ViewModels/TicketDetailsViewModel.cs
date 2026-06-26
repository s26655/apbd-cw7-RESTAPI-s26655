using MiniHelpdesk.Models;

namespace MiniHelpdesk.ViewModels;

public class TicketDetailsViewModel
{
    public Ticket Ticket { get; set; } = new();

    public IEnumerable<TicketComment> Comments { get; set; } = [];
}