using Mercury.Models.Auth;
using Mercury.Models.Jobs;
using Mercury.Models.Movies;
using Mercury.Util;
using Microsoft.EntityFrameworkCore;

namespace Mercury.Db;

public class MysqlContext(DbContextOptions options) : DbContext(options)
{
    // Auth
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }

    public DbSet<Device> Devices { get; set; }

    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    // Jobs

    public DbSet<Job> Jobs { get; set; }

    //Movies
    public DbSet<Movie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed roles
        modelBuilder
            .Entity<Role>()
            .HasData(
                new Role
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Administrator role",
                    Active = true
                },
                new Role
                {
                    Id = 2,
                    Name = "User",
                    Description = "Regular user role",
                    Active = true
                }
            );
        modelBuilder
            .Entity<Permission>()
            .HasData(
                new Permission { Id = (int)PermissionsTypes.AccessMovies, Name = "AccessMovies" },
                new Permission { Id = (int)PermissionsTypes.ReadMovies, Name = "ReadMovies" },
                new Permission { Id = (int)PermissionsTypes.CreateMovies, Name = "CreateMovies" },
                new Permission { Id = (int)PermissionsTypes.UpdateMovies, Name = "UpdateMovies" },
                new Permission { Id = (int)PermissionsTypes.DeleteMovies, Name = "DeleteMovies" }
            );

        modelBuilder
            .Entity<Job>()
            .HasData(
                new Job
                {
                    Id = 1,
                    Name = "LogBackground",
                    Schedule = "0 * * * * ?",
                    JobType = "Mercury.Jobs.LogBackgroundJob"
                }
            );

        // Unique constraints
        modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();

        // modelBuilder.Entity<Session>().HasKey(s => new { s.DeviceId, s.UserId });
    }
}
