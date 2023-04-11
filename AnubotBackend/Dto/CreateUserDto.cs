using System.ComponentModel.DataAnnotations;

namespace AnubotBackend.Dto;

public class CreateUserDto
{
    [Required(AllowEmptyStrings = false)]
    public string UserName { get; set; } = null!;
}
