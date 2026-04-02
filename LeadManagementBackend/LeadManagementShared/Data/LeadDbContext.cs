using Microsoft.EntityFrameworkCore;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Data;

public class LeadDbContext : DbContext
{
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<User> Users { get; set; }

    public LeadDbContext(DbContextOptions<LeadDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>().HasKey(l => l.LeadId);
        modelBuilder.Entity<Interaction>().HasKey(i => i.InteractionId);
        modelBuilder.Entity<User>().HasKey(u => u.UserId);

        // Indexes on Lead: Email, Status, AssignedSalesRepId, Source
        modelBuilder.Entity<Lead>().HasIndex(l => l.Email);
        modelBuilder.Entity<Lead>().HasIndex(l => l.Status);
        modelBuilder.Entity<Lead>().HasIndex(l => l.AssignedSalesRepId);
        modelBuilder.Entity<Lead>().HasIndex(l => l.Source);

        // Index on User.Email (unique)
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Foreign key: Lead → User (SalesRep role, SetNull on delete)
        modelBuilder.Entity<Lead>()
            .HasOne(l => l.AssignedSalesRep)
            .WithMany(u => u.AssignedLeads)
            .HasForeignKey(l => l.AssignedSalesRepId)
            .OnDelete(DeleteBehavior.SetNull);

        // Foreign key: Interaction → Lead (Cascade on delete)
        modelBuilder.Entity<Interaction>()
            .HasOne(i => i.Lead)
            .WithMany(l => l.Interactions)
            .HasForeignKey(i => i.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Lead>().Property(l => l.Email).HasMaxLength(256);
        modelBuilder.Entity<Lead>().Property(l => l.Status).HasMaxLength(50);
        modelBuilder.Entity<Lead>().Property(l => l.Source).HasMaxLength(100);
        modelBuilder.Entity<Lead>().Property(l => l.Priority).HasMaxLength(50);
        modelBuilder.Entity<Lead>().Property(l => l.Name).HasMaxLength(200);

        modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(256);
        modelBuilder.Entity<User>().Property(u => u.Role).HasMaxLength(50);
    }
}
