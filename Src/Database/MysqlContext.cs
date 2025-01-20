using Mercury.Models.Auth;
using Mercury.Models.Movies;
using Microsoft.EntityFrameworkCore;

namespace Mercury.Db;

public class MysqlContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Movie> Movies { get; set; }
    public DbSet<User> Users { get; set; }
}
