using AnubotBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace AnubotBackend.Dto;

/// <summary>
/// 대화 개체 갱신 요청 DTO
/// </summary>
public class UpdateChatDto
{
    /// <summary>
    /// 해당 대화에 대한 유저의 피드백
    /// </summary>
    [EnumDataType(typeof(Feedback))]
    public Feedback Feedback { get; set; }
}
