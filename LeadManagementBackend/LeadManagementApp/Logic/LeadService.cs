// These are the libraries (packages) this file needs to work
using LeadManagementSystem.Models;
using LeadManagementSystem.Data;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Features.Common;

// This file belongs to the "Logic" folder — it contains business rules
namespace LeadManagementSystem.Logic;

// This class handles all the business rules for leads (like changing status and converting)
public class LeadService
{
    // This is how we talk to the database — through a "repository" (a helper that reads/writes data)
    private readonly ILeadRepository _repo;

    // The constructor receives the repository automatically (this is called Dependency Injection)
    public LeadService(ILeadRepository repo)
    {
        _repo = repo;
    }

    // This dictionary defines which status changes are allowed
    // For example: a "New" lead can only move to "Contacted"
    // "Converted" and "Unqualified" leads cannot change status at all
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        ["New"] = new() { "Contacted" },
        ["Contacted"] = new() { "Qualified", "Unqualified" },
        ["Qualified"] = new() { "Converted", "Unqualified" },
        ["Unqualified"] = new(),
        ["Converted"] = new(),
    };

    // This method changes a lead's status (e.g., from "New" to "Contacted")
    // It checks if the transition is allowed before making any change
    public OperationResult UpdateStatus(int leadId, string newStatus)
    {
        // Find the lead in the database
        var lead = _repo.GetLeadById(leadId);
        // If the lead doesn't exist, return an error
        if (lead == null) return OperationResult.Fail("Lead not found.");
        // Converted leads are locked — no changes allowed
        if (lead.Status == "Converted") return OperationResult.Fail("Converted leads cannot be modified.");

        // Check if this status change is in the allowed transitions list
        if (!AllowedTransitions.TryGetValue(lead.Status, out var allowed) || !allowed.Contains(newStatus))
        {
            return OperationResult.Fail($"Cannot transition from {lead.Status} to {newStatus}.");
        }

        // Apply the new status and update the modification timestamp
        lead.Status = newStatus;
        lead.ModifiedDate = DateTime.UtcNow;
        _repo.UpdateLead(lead);
        return OperationResult.Ok($"Status updated to {newStatus}.");
    }

    // This method converts a qualified lead into a customer
    // Only leads with status "Qualified" can be converted
    public OperationResult ConvertToCustomer(int leadId)
    {
        // Find the lead in the database
        var lead = _repo.GetLeadById(leadId);
        
        // Only "Qualified" leads can become customers
        if (lead != null && lead.Status == "Qualified")
        {
            // Mark the lead as converted and record when it happened
            lead.Status = "Converted";
            lead.ConvertedDate = DateTime.UtcNow;
            lead.ModifiedDate = DateTime.UtcNow;
            _repo.UpdateLead(lead);
            // This is where integration with Customer Management would happen
            return OperationResult.Ok("Lead has been converted to a customer.");
        }
        return OperationResult.Fail("Only 'Qualified' leads can be converted.");
    }
}

/*
 * FILE SUMMARY: LeadService.cs
 *
 * This file contains the core business logic for managing leads in the system.
 * It enforces the rules about which status transitions are valid (e.g., a "New" lead
 * can only move to "Contacted", not jump straight to "Converted").
 * It also handles converting a qualified lead into a customer, which is a one-way action.
 * This service is used by the API handlers to ensure all lead changes follow the business rules.
 */
