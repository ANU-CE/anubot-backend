namespace AnubotBackend.Models;

/// <summary>
/// 유저 개체
/// </summary>
public class User
{
    /// <summary>
    /// 유저 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 유저의 이름
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 유저가 생성한 대화 목록
    /// </summary>
    public ICollection<Chat> Chats { get; } = new List<Chat>();

    /// <summary>
    /// 유저 개체가 생성된 시각
    /// </summary>
    public DateTime CreatedDateTime { get; set; }
}
