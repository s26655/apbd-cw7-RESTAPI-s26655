using MiniHelpdesk.Models;
using MiniHelpdesk.ViewModels;

namespace MiniHelpdesk.Services;

public interface ITicketService
{
    Task<IReadOnlyList<Ticket>> GetAllAsync();

    Task<TicketDetailsViewModel?> GetDetailsAsync(int id);

    Task<int> CreateAsync(CreateTicketViewModel model);

    Task<bool> CloseAsync(int id);
}