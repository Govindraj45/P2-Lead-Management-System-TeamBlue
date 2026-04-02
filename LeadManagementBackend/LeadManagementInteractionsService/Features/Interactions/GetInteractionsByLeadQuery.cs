// Import the repository interface and the Interaction model
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Interactions;

// Query record — holds the lead ID we want to fetch interactions for
// In CQRS, queries are read-only operations that never change data
public sealed record GetInteractionsByLeadQuery(int LeadId);

// Handler — contains the logic for fetching interactions from the database
public sealed class GetInteractionsByLeadHandler
{
    // Repository that talks to the database for interaction data
    private readonly IInteractionRepository _repository;

    // Constructor — receives the repository via dependency injection
    public GetInteractionsByLeadHandler(IInteractionRepository repository)
    {
        _repository = repository;
    }

    // Main method — gets all interactions belonging to the given lead ID
    public Task<List<Interaction>> HandleAsync(GetInteractionsByLeadQuery request)
    {
        // Ask the repository for all interactions linked to this lead and return them
        return Task.FromResult(_repository.GetInteractionsByLead(request.LeadId));
    }
}

/*
    FILE SUMMARY:
    This file implements the "Get Interactions By Lead" query in the CQRS pattern.
    The GetInteractionsByLeadQuery record carries the lead ID to look up.
    The GetInteractionsByLeadHandler fetches all interactions for that lead from the database.
    This is a read-only operation — it never creates, updates, or deletes any data.
*/
