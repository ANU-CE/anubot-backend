namespace AnubotBackend.Models;

/// <summary>
/// 대화 개체
/// 이 개체는 사용자와 아누봇의 메시지 쌍을 나타냅니다.
/// </summary>
public class Chat
{
    /// <summary>
    /// 사용자가 아누봇에게 보낸 메시지
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// 아누봇이 사용자에게 보내는 메시지
    /// </summary>
    public required string Reply { get; set; }
}
