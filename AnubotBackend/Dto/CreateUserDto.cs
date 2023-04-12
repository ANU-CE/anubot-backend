using System.ComponentModel.DataAnnotations;

namespace AnubotBackend.Dto;

/// <summary>
/// 유저 개체 생성 요청 DTO
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// 생성할 사용자의 이름
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string UserName { get; set; } = null!;
}
