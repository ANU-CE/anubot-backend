using AnubotBackend.Models;
using Microsoft.EntityFrameworkCore;
using dotenv.net;

namespace AnubotBackend;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        DotEnv.Load();

        optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DB_STRING"));
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Chat> Chats { get; set; }
}
