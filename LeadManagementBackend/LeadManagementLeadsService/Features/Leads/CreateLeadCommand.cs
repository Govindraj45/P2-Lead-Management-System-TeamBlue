// Import the repository interface for saving data to the database
using LeadManagementSystem.Interfaces;
// Import the Lead model (the shape of a lead in the database)
using LeadManagementSystem.Models;
// Import the OperationResult type for returning success/failure responses
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Features.Leads;

// This record holds the data needed to create a new lead (the "command" in CQRS)
public sealed record CreateLeadCommand(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedToRepId);

// This handler does the actual work of creating a lead in the database
public sealed class CreateLeadHandler
{
    // The repository provides methods to read and write leads in the database
    private readonly ILeadRepository _repository;

    // Constructor: .NET injects the repository automatically
    public CreateLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    // Create a new lead from the command data and save it to the database
    public Task<OperationResult<int>> HandleAsync(CreateLeadCommand request)
    {
        // Build a new Lead object with the provided values (use defaults if not provided)
        var lead = new Lead
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            // Default to "New" status if none is provided
            Status = string.IsNullOrWhiteSpace(request.Status) ? "New" : request.Status,
            // Default to "Website" source if none is provided
            Source = string.IsNullOrWhiteSpace(request.Source) ? "Website" : request.Source,
            // Default to "Medium" priority if none is provided
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority,
            AssignedToRepId = request.AssignedToRepId,
            CreatedDate = DateTime.UtcNow
        };

        // Save the lead to the database via the repository
        _repository.AddLead(lead);
        // Return success with the new lead's ID
        return Task.FromResult(OperationResult<int>.Ok(lead.LeadId, "Lead created successfully."));
    }
}

/*
 * FILE SUMMARY:
 * This file contains the CreateLeadCommand (data) and CreateLeadHandler (logic) for creating new leads.
 * The command is a simple data record that carries the lead's details from the controller to the handler.
 * The handler builds a Lead object with sensible defaults, saves it via the repository, and returns the new ID.
 * This follows the CQRS pattern where commands represent actions that change data.
 * It is called from the LeadsController when a POST request is made to /api/leads.
 */
