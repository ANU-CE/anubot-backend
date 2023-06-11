using AnubotBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnubotBackend;

/// <summary>
/// Entity Framework Core의 데이터베이스 컨텍스트.
/// 이 애플리케이션에서 유일한 DbContext 개체입니다.
/// </summary>
public class Context : DbContext
{
    /// <summary>
    /// DB Context 생성자
    /// </summary>
    /// <param name="options">DB Context 생성에 필요한 옵션</param>
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    /// <summary>
    /// 유저 테이블
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 대화 테이블
    /// </summary>
    public DbSet<Chat> Chats { get; set; }
}
