using ES.EventStoreDb.Example.Data;
using Microsoft.EntityFrameworkCore;

namespace ES.EventStoreDb.Example;

public class SqlDbContext : DbContext
{
    public DbSet<People> People { get; set; }
}