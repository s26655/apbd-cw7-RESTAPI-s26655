using System.ComponentModel.DataAnnotations;

namespace MiniHelpdesk.ViewModels;

public class CreateTicketViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Author cannot be longer than 100 characters.")]
    public string? FirstCommentAuthor { get; set; }

    [Required(ErrorMessage = "First comment content is required.")]
    public string FirstCommentContent { get; set; } = string.Empty;
}