namespace AnubotBackend.Models;

/// <summary>
/// 대화 개체
/// 이 개체는 사용자와 아누봇의 메시지 쌍을 나타냅니다.
/// </summary>
public class Chat
{
    /// <summary>
    /// 대화 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 사용자가 아누봇에게 보낸 메시지
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// 아누봇이 사용자에게 보내는 메시지
    /// </summary>
    public required string Reply { get; set; }

    /// <summary>
    /// 메시지를 보낸 사용자의 ID
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// 메시지를 보낸 사용자
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// 아누봇의 답장에 대한 사용자의 피드백
    /// </summary>
    public Feedback? Feedback { get; set; }

    /// <summary>
    /// 대화 개체가 생성된 시각
    /// </summary>
    public DateTime CreatedDateTime { get; set; }
}
