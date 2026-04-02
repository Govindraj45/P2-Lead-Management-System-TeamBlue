// Import the OperationResult type for returning success/failure responses
using LeadManagementSystem.Features.Common;
// Import the repository interface for reading and deleting data
using LeadManagementSystem.Interfaces;

namespace LeadManagementSystem.Features.Leads;

// This record holds the ID of the lead to delete
public sealed record DeleteLeadCommand(int LeadId);

// This handler deletes a lead from the database
public sealed class DeleteLeadHandler
{
    // The repository provides methods to read and delete leads from the database
    private readonly ILeadRepository _repository;

    // Constructor: .NET injects the repository automatically
    public DeleteLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    // Delete a lead by its ID, but first check if it exists
    public Task<OperationResult> HandleAsync(DeleteLeadCommand request)
    {
        // Check if the lead exists before trying to delete it
        var existing = _repository.GetLeadById(request.LeadId);
        if (existing is null)
        {
            return Task.FromResult(OperationResult.Fail("Lead not found."));
        }

        // Remove the lead from the database
        _repository.DeleteLead(request.LeadId);
        return Task.FromResult(OperationResult.Ok("Lead deleted successfully."));
    }
}

/*
 * FILE SUMMARY:
 * This file contains the DeleteLeadCommand (data) and DeleteLeadHandler (logic) for deleting leads.
 * The command carries the lead's ID, and the handler checks if the lead exists before deleting it.
 * If the lead is not found, it returns a failure result; otherwise, it removes the lead from the database.
 * This follows the CQRS pattern where commands represent actions that change data.
 * It is called from the LeadsController when a DELETE request is made to /api/leads/{id}.
 */
