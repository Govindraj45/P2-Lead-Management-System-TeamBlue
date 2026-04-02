// Import the OperationResult type for returning success/failure responses
using LeadManagementSystem.Features.Common;
// Import the repository interface for reading and writing data
using LeadManagementSystem.Interfaces;
// Import the Lead model
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Leads;

// This record holds all the data needed to update an existing lead
public sealed record UpdateLeadCommand(
    int LeadId,
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string Status,
    string Source,
    string Priority,
    int? AssignedToRepId);

// This handler does the actual work of updating a lead in the database
public sealed class UpdateLeadHandler
{
    // The repository provides methods to read and write leads in the database
    private readonly ILeadRepository _repository;

    // Constructor: .NET injects the repository automatically
    public UpdateLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    // Update an existing lead with the new values from the command
    public Task<OperationResult> HandleAsync(UpdateLeadCommand request)
    {
        // First, check if the lead exists in the database
        var existing = _repository.GetLeadById(request.LeadId);
        if (existing is null)
        {
            return Task.FromResult(OperationResult.Fail("Lead not found."));
        }

        // Build a new Lead object with the updated fields, keeping the original CreatedDate
        var updatedLead = new Lead
        {
            LeadId = request.LeadId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Status = request.Status,
            Source = request.Source,
            Priority = request.Priority,
            AssignedToRepId = request.AssignedToRepId,
            // Preserve the original creation date
            CreatedDate = existing.CreatedDate,
            // Preserve the existing interactions
            Interactions = existing.Interactions
        };

        // Save the updated lead to the database
        _repository.UpdateLead(updatedLead);
        return Task.FromResult(OperationResult.Ok("Lead updated successfully."));
    }
}

/*
 * FILE SUMMARY:
 * This file contains the UpdateLeadCommand (data) and UpdateLeadHandler (logic) for updating existing leads.
 * The command carries all the new values for the lead, and the handler applies them to the database.
 * It first checks that the lead exists, then builds a new Lead object preserving the original CreatedDate.
 * This follows the CQRS pattern where commands represent actions that change data.
 * It is called from the LeadsController when a PUT request is made to /api/leads/{id}.
 */
