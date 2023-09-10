using System.ComponentModel.DataAnnotations;

namespace AnubotBackend.Dto;

/// <summary>
/// 대화 생성 요청 DTO
/// </summary>
public class CreateChatDto
{
    /// <summary>
    /// 아누봇에게 보낼 메시지
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string Message { get; set; }
}
