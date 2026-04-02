// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Interactions;

// This is a Query = a question that reads data (it doesn't change anything)
// It takes a LeadId so it knows which lead's interactions to fetch
public sealed record GetInteractionsByLeadQuery(int LeadId);

// This is the handler — it contains the logic to answer the query above
public sealed class GetInteractionsByLeadHandler
{
    // _repository talks to the database to read interaction data
    private readonly IInteractionRepository _repository;

    // The constructor receives the repository tool when this handler is created
    public GetInteractionsByLeadHandler(IInteractionRepository repository)
    {
        _repository = repository;
    }

    // This method runs when someone asks for all interactions belonging to a specific lead
    // It returns a list of every call, email, meeting, etc. recorded for that lead
    public Task<List<Interaction>> HandleAsync(GetInteractionsByLeadQuery request)
    {
        return Task.FromResult(_repository.GetInteractionsByLead(request.LeadId));
    }
}

/*
 * FILE SUMMARY: GetInteractionsByLeadQuery.cs
 *
 * This file handles fetching all interactions for a specific lead (Query = question that reads data).
 * When a user views a lead's detail page, this handler gets the complete communication history —
 * every call, email, and meeting that was recorded for that lead.
 * It's a simple read-only operation that doesn't change any data.
 * This helps sales reps see the full picture of their relationship with each potential customer.
 */
