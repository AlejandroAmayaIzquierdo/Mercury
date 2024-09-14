
using Mercury.Models;
using Microsoft.EntityFrameworkCore;

namespace Mercury.DB;
public class MySqliteContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    public DbSet<Rol> Roles { get; set; }

    protected void OModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne("Rol").WithMany("Users").HasForeignKey("RolID");
        base.OnModelCreating(modelBuilder);
    }

}