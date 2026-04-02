// Import Entity Framework Core — the tool that talks to the database for us
using Microsoft.EntityFrameworkCore;
// Import our model classes so EF Core knows what tables to create
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Data;

// This is the "database context" — the main class that connects our C# code to the SQL database
// It tells Entity Framework Core which tables exist and how they relate to each other
public class LeadDbContext : DbContext
{
    // Each DbSet represents a table in the database
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<User> Users { get; set; }

    // Constructor: receives database connection settings from dependency injection
    public LeadDbContext(DbContextOptions<LeadDbContext> options) : base(options) { }

    // This method runs when EF Core builds the database schema
    // We use it to set up keys, indexes, relationships, and column sizes
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set the primary key for each table
        modelBuilder.Entity<Lead>().HasKey(l => l.LeadId);
        modelBuilder.Entity<Interaction>().HasKey(i => i.InteractionId);
        modelBuilder.Entity<User>().HasKey(u => u.UserId);

        // Create indexes on Lead columns that are frequently searched or filtered
        // Indexes make database queries faster, like an index in a book
        modelBuilder.Entity<Lead>().HasIndex(l => l.Email);
        modelBuilder.Entity<Lead>().HasIndex(l => l.Status);
        modelBuilder.Entity<Lead>().HasIndex(l => l.AssignedSalesRepId);
        modelBuilder.Entity<Lead>().HasIndex(l => l.Source);

        // Make sure no two users can have the same email address
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Relationship: each Lead can be assigned to one User (sales rep)
        // If the sales rep is deleted, set the lead's AssignedSalesRepId to null (don't delete the lead)
        modelBuilder.Entity<Lead>()
            .HasOne(l => l.AssignedSalesRep)
            .WithMany(u => u.AssignedLeads)
            .HasForeignKey(l => l.AssignedSalesRepId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship: each Interaction belongs to one Lead
        // If a lead is deleted, all its interactions are automatically deleted too
        modelBuilder.Entity<Interaction>()
            .HasOne(i => i.Lead)
            .WithMany(l => l.Interactions)
            .HasForeignKey(i => i.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        // Set maximum lengths for string columns to keep the database efficient
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
 * FILE SUMMARY — Data/LeadDbContext.cs (Shared Library)
 * This file is the Entity Framework Core "database context" — the bridge between C# objects and SQL tables.
 * It defines three tables (Leads, Interactions, Users), their primary keys, indexes, relationships,
 * and column constraints like max length and unique email.
 * As part of the shared library, this context is used by all microservices that need direct
 * database access, ensuring they all share the same schema and relationship rules.
 */
