using System.Data;
using Microsoft.Data.SqlClient;
using MiniHelpdesk.Models;

namespace MiniHelpdesk.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly string _connectionString;

    public TicketRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync()
    {
        var tickets = new List<Ticket>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Status, CreatedAt
            FROM dbo.Tickets
            ORDER BY CreatedAt DESC, Id DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tickets.Add(MapTicket(reader));
        }

        return tickets;
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Status, CreatedAt
            FROM dbo.Tickets
            WHERE Id = @Id;
            """;

        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapTicket(reader);
    }

    public async Task<IReadOnlyList<TicketComment>> GetCommentsByTicketIdAsync(int ticketId)
    {
        var comments = new List<TicketComment>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, TicketId, Author, Content, CreatedAt
            FROM dbo.TicketComments
            WHERE TicketId = @TicketId
            ORDER BY CreatedAt ASC, Id ASC;
            """;

        command.Parameters.Add("@TicketId", SqlDbType.Int).Value = ticketId;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            comments.Add(MapTicketComment(reader));
        }

        return comments;
    }

    public async Task<int> CreateWithFirstCommentAsync(Ticket ticket, TicketComment firstComment)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using var ticketCommand = connection.CreateCommand();
            ticketCommand.Transaction = transaction;
            ticketCommand.CommandText = """
                INSERT INTO dbo.Tickets (Title, Description, Status, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@Title, @Description, @Status, @CreatedAt);
                """;

            ticketCommand.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = ticket.Title;
            ticketCommand.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value =
                (object?)ticket.Description ?? DBNull.Value;
            ticketCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = ticket.Status.ToString();
            ticketCommand.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = ticket.CreatedAt;

            var newTicketIdObject = await ticketCommand.ExecuteScalarAsync();
            var newTicketId = Convert.ToInt32(newTicketIdObject);

            await using var commentCommand = connection.CreateCommand();
            commentCommand.Transaction = transaction;
            commentCommand.CommandText = """
                INSERT INTO dbo.TicketComments (TicketId, Author, Content, CreatedAt)
                VALUES (@TicketId, @Author, @Content, @CreatedAt);
                """;

            commentCommand.Parameters.Add("@TicketId", SqlDbType.Int).Value = newTicketId;
            commentCommand.Parameters.Add("@Author", SqlDbType.NVarChar, 100).Value = firstComment.Author;
            commentCommand.Parameters.Add("@Content", SqlDbType.NVarChar, -1).Value = firstComment.Content;
            commentCommand.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = firstComment.CreatedAt;

            await commentCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return newTicketId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> CloseAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.Tickets
            SET Status = @Status
            WHERE Id = @Id;
            """;

        command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = TicketStatus.Closed.ToString();
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        var affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    private static Ticket MapTicket(SqlDataReader reader)
    {
        return new Ticket
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            Status = Enum.Parse<TicketStatus>(reader.GetString(3)),
            CreatedAt = reader.GetDateTime(4)
        };
    }

    private static TicketComment MapTicketComment(SqlDataReader reader)
    {
        return new TicketComment
        {
            Id = reader.GetInt32(0),
            TicketId = reader.GetInt32(1),
            Author = reader.GetString(2),
            Content = reader.GetString(3),
            CreatedAt = reader.GetDateTime(4)
        };
    }
}