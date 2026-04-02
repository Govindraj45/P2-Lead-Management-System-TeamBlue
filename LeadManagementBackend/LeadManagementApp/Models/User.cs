// These "using" statements bring in tools for marking fields as database columns
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// This file belongs to the "Models" folder — it defines what a User looks like
namespace LeadManagementSystem.Models;

// A "User" is someone who logs into the system — could be a salesperson, manager, or admin
public class User
{
    // [Key] marks this as the unique ID for each user in the database
    // [DatabaseGenerated] means the database auto-assigns this number
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    // The user's login email address — also used as their username
    // "null!" means this field is required and must always have a value
    public string Email { get; set; } = null!;

    // The user's password stored as a hash (scrambled for security — never stored as plain text)
    public string PasswordHash { get; set; } = null!;

    // The user's display name (e.g., "John Smith")
    public string FullName { get; set; } = null!;

    // What permissions does this user have? Options: SalesRep, SalesManager, or Admin
    // SalesRep = can manage their own leads; SalesManager = can see all leads; Admin = full control
    public string Role { get; set; } = null!;

    // When was this user account created?
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // All the leads assigned to this user (only relevant when the user is a SalesRep)
    // This is a "one-to-many" relationship: 1 User → many Leads
    public virtual ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
}

/*
 * FILE SUMMARY: Models/User.cs
 * 
 * This file defines the User model — it represents anyone who can log into the system.
 * Users have roles (SalesRep, SalesManager, Admin) that control what they can see and do.
 * The password is stored as a secure hash, never as plain text.
 * Sales reps are linked to leads through the AssignedLeads collection, which tracks
 * which potential customers each salesperson is responsible for.
 */
