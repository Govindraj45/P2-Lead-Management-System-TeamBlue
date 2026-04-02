// These "using" statements bring in tools for marking fields as database columns
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// This file belongs to the "Models" folder — it defines what an Interaction looks like
namespace LeadManagementSystem.Models;

// An "Interaction" is any contact a salesperson has with a lead (a phone call, email, or meeting)
public class Interaction
{
    // [Key] marks this as the unique ID for each interaction in the database
    // [DatabaseGenerated] means the database auto-assigns this number
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InteractionId { get; set; }

    // What kind of interaction was it? Options: Call, Email, or Meeting
    public string InteractionType { get; set; } = string.Empty;

    // Free-text notes about what happened during this interaction
    public string Notes { get; set; } = string.Empty;

    // When did this interaction take place?
    public DateTime InteractionDate { get; set; } = DateTime.Now;

    // When should the salesperson follow up? (optional — null means no follow-up scheduled)
    public DateTime? FollowUpDate { get; set; }

    // Which lead does this interaction belong to?
    // This is a "foreign key" — it links back to a specific Lead record
    public int LeadId { get; set; }

    // This gives us direct access to the full Lead object
    // "virtual" allows Entity Framework to load the lead data automatically when needed
    public virtual Lead? Lead { get; set; }
}

/*
 * FILE SUMMARY: Models/Interaction.cs
 * 
 * This file defines the Interaction model — it represents every time a salesperson
 * contacts a potential customer (lead). Each interaction records the type (call, email,
 * meeting), notes about the conversation, and an optional follow-up date.
 * Every interaction is linked to exactly one lead, forming a history of all communications.
 * This data helps managers track how actively leads are being pursued.
 */
