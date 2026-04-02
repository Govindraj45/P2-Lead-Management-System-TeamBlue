// Import tools for marking database fields like [Key] and [Required]
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// This class lives in the shared Models namespace so all microservices can use it
namespace LeadManagementSystem.Models;

// This class represents a "User" — someone who logs into the system (SalesRep, SalesManager, or Admin)
public class User
{
    // Primary key: a unique number that identifies each user in the database
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    // The user's login email address (must be unique)
    public string Email { get; set; } = null!;

    // The hashed version of the user's password (never store plain text passwords!)
    public string PasswordHash { get; set; } = null!;

    // The user's display name
    public string FullName { get; set; } = null!;

    // The user's role determines what they can do in the system
    public string Role { get; set; } = null!; // SalesRep, SalesManager, Admin

    // When the user account was created
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation: all the leads assigned to this user (only applies when Role is SalesRep)
    public virtual ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
}

/*
 * FILE SUMMARY — Models/User.cs (Shared Library)
 * This file defines the User model, which represents anyone who can log into the system.
 * Users have one of three roles: SalesRep (works leads), SalesManager (oversees reps), or Admin (full access).
 * The model stores login credentials (email + hashed password), the user's name, and their role.
 * As part of the shared library, this model is used across all microservices for authentication
 * and for linking sales reps to their assigned leads.
 */
