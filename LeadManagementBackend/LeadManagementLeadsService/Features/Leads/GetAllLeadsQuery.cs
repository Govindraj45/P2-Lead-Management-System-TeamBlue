// Import the repository interface for reading data from the database
using LeadManagementSystem.Interfaces;
// Import the Lead model
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Leads;

// This record represents a query to get all leads (empty because no filters are needed)
public sealed record GetAllLeadsQuery();

// This handler retrieves all leads from the database
public sealed class GetAllLeadsHandler
{
    // The repository provides methods to read leads from the database
    private readonly ILeadRepository _repository;

    // Constructor: .NET injects the repository automatically
    public GetAllLeadsHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    // Fetch all leads from the database and return them as a list
    public Task<List<Lead>> HandleAsync(GetAllLeadsQuery request)
    {
        return Task.FromResult(_repository.GetAllLeads());
    }
}

/*
 * FILE SUMMARY:
 * This file contains the GetAllLeadsQuery (data) and GetAllLeadsHandler (logic) for fetching all leads.
 * The query is empty because we don't need any filters — we just want every lead in the database.
 * The handler asks the repository for all leads and returns them as a list.
 * This follows the CQRS pattern where queries represent read-only operations that don't change data.
 * It is called from the LeadsController when a GET request is made to /api/leads.
 */
