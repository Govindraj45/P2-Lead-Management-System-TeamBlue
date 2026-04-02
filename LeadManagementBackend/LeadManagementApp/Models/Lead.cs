// These "using" statements bring in tools for marking fields as database columns
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// This file belongs to the "Models" folder — it defines what a Lead looks like in our system
namespace LeadManagementSystem.Models;

// A "Lead" is a potential customer that a sales team is trying to convert into a real customer
public class Lead 
{
    // [Key] marks this field as the primary key — the unique ID for each lead in the database
    // [DatabaseGenerated] means the database will auto-create this number (1, 2, 3, etc.)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LeadId { get; set; }

    // Basic contact information about the lead
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }        // The "?" means this field is optional (can be empty)
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Position { get; set; }

    // The current stage of this lead in the sales process
    // It starts as "New" and can move through: New → Contacted → Qualified → Converted (or Unqualified)
    public string Status { get; set; } = "New";

    // Where did this lead come from? (e.g., Website, Referral, LinkedIn, etc.)
    public string Source { get; set; } = "Website";

    // How important is this lead? Can be Low, Medium, or High
    public string Priority { get; set; } = "Medium";

    // When was this lead first created?
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // When was this lead last updated? (null if never updated)
    public DateTime? ModifiedDate { get; set; }

    // When was this lead converted into a real customer? (null if not converted yet)
    public DateTime? ConvertedDate { get; set; }

    // Which salesperson is responsible for this lead?
    // This is a "foreign key" — it links to a User record in the Users table
    public int? AssignedSalesRepId { get; set; }

    // This gives us direct access to the full User object for the assigned sales rep
    // "virtual" allows Entity Framework to load this data automatically when needed
    public virtual User? AssignedSalesRep { get; set; }

    // A lead can have many interactions (calls, emails, meetings)
    // This is a "one-to-many" relationship: 1 Lead → many Interactions
    public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
}

/*
 * FILE SUMMARY: Models/Lead.cs
 * 
 * This file defines the Lead model — the core data structure of the entire application.
 * A Lead represents a potential customer that the sales team is tracking and trying to convert.
 * It stores contact info (name, email, phone), sales info (status, source, priority),
 * and links to other data like the assigned salesperson and all related interactions.
 * Every feature in the app — creating leads, updating statuses, generating reports — depends on this model.
 */
