// Import the shared OperationResult helper and repository interface
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;

namespace LeadManagementSystem.Features.SalesReps;

// Command record — holds the ID of the sales rep to delete
public sealed record DeleteSalesRepCommand(int RepId);

// Handler — contains the business logic for deleting a sales rep
public sealed class DeleteSalesRepHandler
{
    // Repository that talks to the database for sales rep data
    private readonly ISalesRepository _repository;

    // Constructor — receives the repository via dependency injection
    public DeleteSalesRepHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    // Main method — processes the delete command and returns success or failure
    public Task<OperationResult> HandleAsync(DeleteSalesRepCommand request)
    {
        // First, check if the sales rep actually exists in the database
        var existing = _repository.GetRepById(request.RepId);
        if (existing is null)
        {
            // If the rep doesn't exist, return a failure result instead of crashing
            return Task.FromResult(OperationResult.Fail("Sales representative not found."));
        }

        // Remove the sales rep from the database
        _repository.DeleteRep(request.RepId);
        // Return a success result
        return Task.FromResult(OperationResult.Ok("Sales representative deleted successfully."));
    }
}

/*
    FILE SUMMARY:
    This file implements the "Delete Sales Rep" command in the CQRS pattern.
    The command carries the rep's ID indicating which sales rep to remove.
    The handler first verifies the rep exists, then deletes them from the database.
    If the rep is not found, it returns a failure message instead of throwing an exception.
    This keeps the delete operation safe and predictable.
*/
