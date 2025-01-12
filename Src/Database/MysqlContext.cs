using Mercury.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace Mercury.Db;

public class MysqlContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Movie> Movies { get; set; }
}
