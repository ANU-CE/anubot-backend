using System.ComponentModel.DataAnnotations.Schema;

namespace AnubotBackend.Models;

public class Chat
{
    public Guid Id { get; set; }

    public required string Message { get; set; }

    public required string Reply { get; set; }

    public required Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Feedback? Feedback { get; set; }

    public DateTime CreatedDateTime { get; set; }
}
