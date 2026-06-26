using MiniHelpdesk.Models;
using MiniHelpdesk.Repositories;
using MiniHelpdesk.Services;
using MiniHelpdesk.ViewModels;

namespace MiniHelpdesk.Tests;

public class TicketServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidInput_CreatesTicketWithFirstComment()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var model = new CreateTicketViewModel
        {
            Title = "Cannot log in",
            Description = "The user cannot log in to the application.",
            FirstCommentAuthor = "Adrian",
            FirstCommentContent = "The issue started this morning."
        };

        // Act
        var ticketId = await service.CreateAsync(model);

        // Assert
        var ticket = await repository.GetByIdAsync(ticketId);
        var comments = await repository.GetCommentsByTicketIdAsync(ticketId);

        Assert.NotNull(ticket);
        Assert.Equal("Cannot log in", ticket.Title);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Single(comments);
        Assert.Equal("Adrian", comments[0].Author);
        Assert.Equal("The issue started this morning.", comments[0].Content);
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsBusinessRuleException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var model = new CreateTicketViewModel
        {
            Title = "   ",
            Description = "Description",
            FirstCommentAuthor = "Adrian",
            FirstCommentContent = "Initial comment"
        };

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(model));

        // Assert
        Assert.Equal("Title is required.", exception.Message);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task CreateAsync_EmptyFirstCommentContent_ThrowsBusinessRuleException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var model = new CreateTicketViewModel
        {
            Title = "Printer does not work",
            Description = "Office printer issue",
            FirstCommentAuthor = "Adrian",
            FirstCommentContent = "   "
        };

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(model));

        // Assert
        Assert.Equal("First comment content is required.", exception.Message);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task CloseAsync_ExistingTicket_ChangesStatusToClosed()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var ticketId = await service.CreateAsync(new CreateTicketViewModel
        {
            Title = "VPN issue",
            Description = "VPN disconnects every few minutes.",
            FirstCommentAuthor = "Adrian",
            FirstCommentContent = "The issue happens on macOS."
        });

        // Act
        var result = await service.CloseAsync(ticketId);

        // Assert
        var ticket = await repository.GetByIdAsync(ticketId);

        Assert.True(result);
        Assert.NotNull(ticket);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    private class FakeTicketRepository : ITicketRepository
    {
        private readonly List<Ticket> _tickets = [];
        private readonly List<TicketComment> _comments = [];
        private int _nextTicketId = 1;
        private int _nextCommentId = 1;

        public Task<IReadOnlyList<Ticket>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<Ticket>>(_tickets);
        }

        public Task<Ticket?> GetByIdAsync(int id)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);

            return Task.FromResult(ticket);
        }

        public Task<IReadOnlyList<TicketComment>> GetCommentsByTicketIdAsync(int ticketId)
        {
            var comments = _comments
                .Where(c => c.TicketId == ticketId)
                .ToList();

            return Task.FromResult<IReadOnlyList<TicketComment>>(comments);
        }

        public Task<int> CreateWithFirstCommentAsync(Ticket ticket, TicketComment firstComment)
        {
            ticket.Id = _nextTicketId++;
            firstComment.Id = _nextCommentId++;
            firstComment.TicketId = ticket.Id;

            _tickets.Add(ticket);
            _comments.Add(firstComment);

            return Task.FromResult(ticket.Id);
        }

        public Task<bool> CloseAsync(int id)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);

            if (ticket is null)
            {
                return Task.FromResult(false);
            }

            ticket.Status = TicketStatus.Closed;

            return Task.FromResult(true);
        }
    }
}