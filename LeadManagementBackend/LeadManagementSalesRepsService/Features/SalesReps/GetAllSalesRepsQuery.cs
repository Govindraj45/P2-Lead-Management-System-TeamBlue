// Import the repository interface and the SalesRep model
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.SalesReps;

// Query record — takes no parameters because it returns ALL sales reps
// In CQRS, queries are read-only operations that never change data
public sealed record GetAllSalesRepsQuery();

// Handler — contains the logic for fetching all sales representatives from the database
public sealed class GetAllSalesRepsHandler
{
    // Repository that talks to the database for sales rep data
    private readonly ISalesRepository _repository;

    // Constructor — receives the repository via dependency injection
    public GetAllSalesRepsHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    // Main method — returns a list of every sales representative in the system
    public Task<List<SalesRep>> HandleAsync(GetAllSalesRepsQuery request)
    {
        return Task.FromResult(_repository.GetAllReps());
    }
}

/*
    FILE SUMMARY:
    This file implements the "Get All Sales Reps" query in the CQRS pattern.
    The query takes no parameters since it fetches the complete list of sales representatives.
    The handler simply calls the repository to retrieve all reps from the database.
    This is a read-only operation — it never creates, updates, or deletes any data.
*/
