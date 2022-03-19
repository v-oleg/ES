using Microsoft.EntityFrameworkCore;

namespace ES.EventStoreDb.Example.Sql;

public class SqlDbContext : DbContext
{
    public SqlDbContext()
    {
        
    }

    public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
    {
        
    }

    public DbSet<PersonMailingAddress> PersonMailingAddress { get; set; } = null!;
    public DbSet<Checkpoints> Checkpoints { get; set; } = null!;
}