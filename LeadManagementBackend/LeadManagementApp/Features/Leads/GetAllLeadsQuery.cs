// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Leads;

// This is a Query = a question that reads data (it doesn't change anything)
// It takes no parameters because it simply asks "give me ALL the leads"
public sealed record GetAllLeadsQuery();

// This is the handler — it contains the logic to answer the query above
public sealed class GetAllLeadsHandler
{
    // _repository talks to the database to read lead data
    private readonly ILeadRepository _repository;

    // The constructor receives the repository tool when this handler is created
    public GetAllLeadsHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    // This method runs when someone asks for all leads — it returns a list of every lead
    public Task<List<Lead>> HandleAsync(GetAllLeadsQuery request)
    {
        return Task.FromResult(_repository.GetAllLeads());
    }
}

/*
 * FILE SUMMARY: GetAllLeadsQuery.cs
 *
 * This file handles fetching every lead from the database (Query = question that reads data).
 * It's very simple — when called, it asks the repository "give me all the leads" and returns the list.
 * This is used whenever the frontend needs to display the full list of leads, like on the main dashboard.
 * No filtering or pagination happens here; it returns everything.
 */
