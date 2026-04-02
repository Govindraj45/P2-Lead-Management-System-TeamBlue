// Import repository interfaces, models, and the shared OperationResult helper
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Features.SalesReps;

// Command record — holds the data needed to create a new sales representative
// Department is optional (nullable) — defaults to "Sales" if not provided
public sealed record CreateSalesRepCommand(
    string Name,
    string Email,
    string? Department);

// Handler — contains the business logic for creating a sales rep
public sealed class CreateSalesRepHandler
{
    // Repository that talks to the database for sales rep data
    private readonly ISalesRepository _repository;

    // Constructor — receives the repository via dependency injection
    public CreateSalesRepHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    // Main method — processes the command and returns success with the new rep's ID
    public Task<OperationResult<int>> HandleAsync(CreateSalesRepCommand request)
    {
        // Build a new SalesRep object from the command data
        var rep = new SalesRep
        {
            Name = request.Name,
            Email = request.Email,
            // If no department is given (null or whitespace), default to "Sales"
            Department = string.IsNullOrWhiteSpace(request.Department) ? "Sales" : request.Department
        };

        // Save the new sales rep to the database
        _repository.AddSalesRep(rep);
        // Return success with the new rep's auto-generated ID
        return Task.FromResult(OperationResult<int>.Ok(rep.RepId, "Sales representative created successfully."));
    }
}

/*
    FILE SUMMARY:
    This file implements the "Create Sales Rep" command in the CQRS pattern.
    The CreateSalesRepCommand record carries the name, email, and optional department.
    The handler builds a SalesRep object, defaults the department to "Sales" if not provided,
    saves it to the database, and returns a success result with the new rep's ID.
*/
