// Entity Framework Core is the tool that talks to the database for us
using Microsoft.EntityFrameworkCore;
// Bring in our model classes so the database knows what tables to create
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Data;

// The "DbContext" is the main bridge between our C# code and the SQL Server database
// Think of it as a manager that handles all database conversations
public class LeadDbContext : DbContext
{
    // These three "DbSet" properties represent the three tables in our database
    // DbSet<Lead> = the "Leads" table, DbSet<Interaction> = the "Interactions" table, etc.
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<User> Users { get; set; }

    // This constructor receives database connection settings from the app's configuration
    // "base(options)" passes those settings up to Entity Framework so it knows where the database is
    public LeadDbContext(DbContextOptions<LeadDbContext> options) : base(options) { }

    // This method lets us customize how the database tables are built
    // It runs once when the database is first created or when migrations are applied
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tell the database which field is the unique identifier (primary key) for each table
        modelBuilder.Entity<Lead>().HasKey(l => l.LeadId);
        modelBuilder.Entity<Interaction>().HasKey(i => i.InteractionId);
        modelBuilder.Entity<User>().HasKey(u => u.UserId);

        // Create "indexes" on Lead columns — these make searching by these fields much faster
        // Think of indexes like a book's table of contents: they help the database find data quickly
        modelBuilder.Entity<Lead>().HasIndex(l => l.Email);
        modelBuilder.Entity<Lead>().HasIndex(l => l.Status);
        modelBuilder.Entity<Lead>().HasIndex(l => l.AssignedSalesRepId);
        modelBuilder.Entity<Lead>().HasIndex(l => l.Source);

        // Make sure no two users can have the same email address (unique index)
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Set up the relationship: each Lead can be assigned to one User (sales rep)
        // If the sales rep is deleted, the lead's AssignedSalesRepId becomes null (SetNull)
        modelBuilder.Entity<Lead>()
            .HasOne(l => l.AssignedSalesRep)
            .WithMany(u => u.AssignedLeads)
            .HasForeignKey(l => l.AssignedSalesRepId)
            .OnDelete(DeleteBehavior.SetNull);

        // Set up the relationship: each Interaction belongs to one Lead
        // If a lead is deleted, all its interactions are also deleted (Cascade)
        modelBuilder.Entity<Interaction>()
            .HasOne(i => i.Lead)
            .WithMany(l => l.Interactions)
            .HasForeignKey(i => i.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        // Set maximum lengths for text columns — this prevents storing absurdly long strings
        // and helps the database optimize storage
        modelBuilder.Entity<Lead>().Property(l => l.Email).HasMaxLength(256);
        modelBuilder.Entity<Lead>().Property(l => l.Status).HasMaxLength(50);
        modelBuilder.Entity<Lead>().Property(l => l.Source).HasMaxLength(100);
        modelBuilder.Entity<Lead>().Property(l => l.Priority).HasMaxLength(50);
        modelBuilder.Entity<Lead>().Property(l => l.Name).HasMaxLength(200);

        modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(256);
        modelBuilder.Entity<User>().Property(u => u.Role).HasMaxLength(50);
    }
}

/*
 * FILE SUMMARY: Data/LeadDbContext.cs
 * 
 * This file is the heart of the database layer — it tells Entity Framework how to create
 * and manage the SQL Server database. It defines three tables (Leads, Interactions, Users),
 * sets up relationships between them (which lead belongs to which sales rep, which interactions
 * belong to which lead), creates indexes for fast searching, and enforces rules like unique emails.
 * Every database operation in the app flows through this context class.
 */