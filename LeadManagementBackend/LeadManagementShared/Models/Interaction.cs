// Import tools for marking database fields like [Key] and [Required]
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// This class lives in the shared Models namespace so all microservices can use it
namespace LeadManagementSystem.Models;

// This class represents an "Interaction" — a recorded contact with a lead (call, email, meeting)
public class Interaction
{
    // Primary key: a unique number that identifies each interaction in the database
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InteractionId { get; set; }

    // The type of contact made (e.g., "Call", "Email", "Meeting")
    public string InteractionType { get; set; } = string.Empty; // Call, Email, Meeting

    // Free-text notes about what happened during the interaction
    public string Notes { get; set; } = string.Empty;

    // When this interaction took place
    public DateTime InteractionDate { get; set; } = DateTime.Now;

    // Optional: a future date to follow up with this lead
    public DateTime? FollowUpDate { get; set; }

    // Foreign Key: links this interaction to the lead it belongs to
    public int LeadId { get; set; }
    public virtual Lead? Lead { get; set; }
}

/*
 * FILE SUMMARY — Models/Interaction.cs (Shared Library)
 * This file defines the Interaction model, which tracks every contact made with a lead.
 * Each interaction records the type (Call, Email, Meeting), notes, date, and an optional follow-up date.
 * It is linked to a Lead via a foreign key, creating a one-to-many relationship (one lead, many interactions).
 * As part of the shared library, this model is reused by the Leads and Interactions microservices
 * so they share the same data structure.
 */
