using System.ComponentModel.DataAnnotations;

namespace AnubotBackend.Dto;

public class CreateChatDto
{
    [Required(AllowEmptyStrings = false)]
    public string Message { get; set; } = null!;

    public Guid UserId { get; set; }
}
