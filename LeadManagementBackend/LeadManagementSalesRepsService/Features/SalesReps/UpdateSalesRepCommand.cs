// Import the shared OperationResult helper, repository interface, and SalesRep model
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.SalesReps;

// Command record — holds the data needed to update an existing sales representative
// RepId identifies which rep to update; Name, Email, Department are the new values
public sealed record UpdateSalesRepCommand(
    int RepId,
    string Name,
    string Email,
    string Department);

// Handler — contains the business logic for updating a sales rep
public sealed class UpdateSalesRepHandler
{
    // Repository that talks to the database for sales rep data
    private readonly ISalesRepository _repository;

    // Constructor — receives the repository via dependency injection
    public UpdateSalesRepHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    // Main method — processes the update command and returns success or failure
    public Task<OperationResult> HandleAsync(UpdateSalesRepCommand request)
    {
        // First, check if the sales rep actually exists in the database
        var existing = _repository.GetRepById(request.RepId);
        if (existing is null)
        {
            // If the rep doesn't exist, return a failure result
            return Task.FromResult(OperationResult.Fail("Sales representative not found."));
        }

        // Build an updated SalesRep object with the new values, keeping existing assigned leads
        var rep = new SalesRep
        {
            RepId = request.RepId,
            Name = request.Name,
            Email = request.Email,
            Department = request.Department,
            AssignedLeads = existing.AssignedLeads
        };

        // Save the updated rep to the database
        _repository.UpdateRep(rep);
        // Return a success result
        return Task.FromResult(OperationResult.Ok("Sales representative updated successfully."));
    }
}

/*
    FILE SUMMARY:
    This file implements the "Update Sales Rep" command in the CQRS pattern.
    The command carries the rep's ID along with new name, email, and department values.
    The handler first checks that the rep exists, then builds an updated object preserving
    the existing assigned leads, saves it to the database, and returns success or failure.
    If the rep is not found, it returns a failure message instead of throwing an exception.
*/
