// Import tools for marking database fields like [Key] and [Required]
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// This class lives in the shared Models namespace so all microservices can use it
namespace LeadManagementSystem.Models;

// This class represents a "Lead" — a potential customer in the sales pipeline
public class Lead 
{
    // Primary key: a unique number that identifies each lead in the database
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LeadId { get; set; }

    // Basic contact information for the lead
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Position { get; set; }

    // The current stage of this lead in the sales pipeline
    public string Status { get; set; } = "New"; // New, Contacted, Qualified, Unqualified, Converted

    // Where the lead came from (e.g., Website, Referral, Advertisement)
    public string Source { get; set; } = "Website";

    // How important this lead is (Low, Medium, High)
    public string Priority { get; set; } = "Medium";

    // Timestamps to track when the lead was created, last changed, or converted
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }

    // Foreign Key: links this lead to the sales rep (User) who is responsible for it
    public int? AssignedSalesRepId { get; set; }
    public virtual User? AssignedSalesRep { get; set; }

    // One lead can have many interactions (calls, emails, meetings)
    public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
}

/*
 * FILE SUMMARY — Models/Lead.cs (Shared Library)
 * This file defines the Lead model, which is the core entity in the Lead Management System.
 * It stores all the information about a potential customer, including their contact details,
 * current sales pipeline status, priority, and which sales rep is assigned to them.
 * As part of the shared library, this model is used by all microservices (Leads, Interactions,
 * Reports, SalesReps) so they all agree on the same data structure for a Lead.
 */
