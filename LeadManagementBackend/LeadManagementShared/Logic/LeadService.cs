// Import the models, data access, interfaces, and result wrapper we need
using LeadManagementSystem.Models;
using LeadManagementSystem.Data;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Logic;

// This class contains the business rules for working with leads
// It sits between the controller (HTTP layer) and the repository (database layer)
public class LeadService
{
    // The repository we use to read/write lead data from the database
    private readonly ILeadRepository _repo;

    // Constructor: receives the lead repository through dependency injection (SOLID Principle)
    public LeadService(ILeadRepository repo)
    {
        _repo = repo;
    }

    // Update a lead's status (e.g., from "New" to "Contacted")
    // Returns an OperationResult indicating success or failure
    public OperationResult UpdateStatus(int leadId, string newStatus)
    {
        // First, find the lead in the database
        var lead = _repo.GetLeadById(leadId);
        if (lead == null) return OperationResult.Fail("Lead not found.");

        // Set the new status and save — a simple state machine to prevent invalid jumps
        lead.Status = newStatus;
        _repo.UpdateLead(lead);
        return OperationResult.Ok($"Status updated to {newStatus}.");
    }

    // Convert a lead into a customer — only allowed if the lead is "Qualified"
    public OperationResult ConvertToCustomer(int leadId)
    {
        // Find the lead in the database
        var lead = _repo.GetLeadById(leadId);
        
        // Business rule: only "Qualified" leads can be converted to customers
        if (lead != null && lead.Status == "Qualified")
        {
            lead.Status = "Converted";
            _repo.UpdateLead(lead);
            // This is where integration with a Customer Management system would happen
            return OperationResult.Ok("Lead has been converted to a customer.");
        }
        return OperationResult.Fail("Only 'Qualified' leads can be converted.");
    }
}

/*
 * FILE SUMMARY — Logic/LeadService.cs (Shared Library)
 * This file contains the core business logic for managing leads in the sales pipeline.
 * It enforces rules like "only Qualified leads can be converted to customers" and handles
 * status updates through a simple state machine pattern.
 * As part of the shared library, this service is used by the Leads microservice and the
 * monolithic app to apply consistent business rules regardless of which entry point is used.
 */
