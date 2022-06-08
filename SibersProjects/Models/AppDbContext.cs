using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SibersProjects.Models;

public class AppDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, IdentityUserRole<string>,
    IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    private readonly ILoggerFactory _loggerFactory;


    public AppDbContext(DbContextOptions<AppDbContext> options, ILoggerFactory loggerFactory) : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    // EF сделает все за нас поэтому просто поставим null чтобы IDE не ругалось
    public virtual DbSet<Project> Projects { get; set; } = null!;
    public virtual DbSet<ProjectAssignment> Assignments { get; set; } = null!;
    public virtual DbSet<WorkTask> Tasks { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(_loggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<IdentityUserRole<string>>();

        builder.Entity<User>()
            .HasMany(e => e.Projects)
            .WithMany(p => p.Employees)
            .UsingEntity<ProjectAssignment>();

        builder.Entity<Project>().HasOne(p => p.ProjectManager);
    }
}