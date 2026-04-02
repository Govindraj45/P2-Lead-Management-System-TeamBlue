// Import the repository interface and the SalesRep model
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.SalesReps;

// Query record — holds the ID of the sales rep we want to look up
public sealed record GetSalesRepByIdQuery(int RepId);

// Handler — contains the logic for fetching a single sales rep by their ID
public sealed class GetSalesRepByIdHandler
{
    // Repository that talks to the database for sales rep data
    private readonly ISalesRepository _repository;

    // Constructor — receives the repository via dependency injection
    public GetSalesRepByIdHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    // Main method — returns the matching sales rep, or null if no rep has that ID
    public Task<SalesRep?> HandleAsync(GetSalesRepByIdQuery request)
    {
        return Task.FromResult(_repository.GetRepById(request.RepId));
    }
}

/*
    FILE SUMMARY:
    This file implements the "Get Sales Rep By ID" query in the CQRS pattern.
    The query carries the rep ID to look up in the database.
    The handler asks the repository for the matching sales rep and returns it (or null if not found).
    This is a read-only operation — it never creates, updates, or deletes any data.
*/
