using MiniHelpdesk.Models;

namespace MiniHelpdesk.Repositories;

public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> GetAllAsync();

    Task<Ticket?> GetByIdAsync(int id);

    Task<IReadOnlyList<TicketComment>> GetCommentsByTicketIdAsync(int ticketId);

    Task<int> CreateWithFirstCommentAsync(Ticket ticket, TicketComment firstComment);

    Task<bool> CloseAsync(int id);
}