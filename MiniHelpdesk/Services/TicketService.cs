using MiniHelpdesk.Models;
using MiniHelpdesk.Repositories;
using MiniHelpdesk.ViewModels;

namespace MiniHelpdesk.Services;

public class TicketService : ITicketService
{
    private const string DefaultAuthor = "Anonymous";

    private readonly ITicketRepository _repository;

    public TicketService(ITicketRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Ticket>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public async Task<TicketDetailsViewModel?> GetDetailsAsync(int id)
    {
        var ticket = await _repository.GetByIdAsync(id);

        if (ticket is null)
        {
            return null;
        }

        var comments = await _repository.GetCommentsByTicketIdAsync(id);

        return new TicketDetailsViewModel
        {
            Ticket = ticket,
            Comments = comments
        };
    }

    public async Task<int> CreateAsync(CreateTicketViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            throw new BusinessRuleException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(model.FirstCommentContent))
        {
            throw new BusinessRuleException("First comment content is required.");
        }

        var createdAt = DateTime.UtcNow;

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim(),
            Status = TicketStatus.Open,
            CreatedAt = createdAt
        };

        var firstComment = new TicketComment
        {
            Author = string.IsNullOrWhiteSpace(model.FirstCommentAuthor)
                ? DefaultAuthor
                : model.FirstCommentAuthor.Trim(),
            Content = model.FirstCommentContent.Trim(),
            CreatedAt = createdAt
        };

        return await _repository.CreateWithFirstCommentAsync(ticket, firstComment);
    }

    public Task<bool> CloseAsync(int id)
    {
        return _repository.CloseAsync(id);
    }
}