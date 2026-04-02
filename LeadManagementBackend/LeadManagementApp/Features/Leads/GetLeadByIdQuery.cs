// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Leads;

// This is a Query = a question that reads data (it doesn't change anything)
// It takes a LeadId so it knows which specific lead to look up
public sealed record GetLeadByIdQuery(int LeadId);

// This is the handler — it contains the logic to answer the query above
public sealed class GetLeadByIdHandler
{
    // _repository talks to the database to read lead data
    private readonly ILeadRepository _repository;

    // The constructor receives the repository tool when this handler is created
    public GetLeadByIdHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    // This method runs when someone asks for one specific lead by its ID
    // It returns the lead if found, or null if no lead has that ID
    public Task<Lead?> HandleAsync(GetLeadByIdQuery request)
    {
        return Task.FromResult(_repository.GetLeadById(request.LeadId));
    }
}

/*
 * FILE SUMMARY: GetLeadByIdQuery.cs
 *
 * This file handles fetching a single lead by its unique ID (Query = question that reads data).
 * When someone wants to view the details of one specific lead, this handler looks it up in the database.
 * If the lead exists, it returns the full lead object; if not, it returns null.
 * This is typically used when a user clicks on a lead in the frontend to see its details.
 */
