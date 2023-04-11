using System.ComponentModel.DataAnnotations.Schema;

namespace AnubotBackend.Models;

public class User
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Chat> Chats { get; } = new List<Chat>();

    public DateTime CreatedDateTime { get; set; }
}
