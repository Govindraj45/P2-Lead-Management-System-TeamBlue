// Import the repository interface for reading data from the database
using LeadManagementSystem.Interfaces;
// Import the Lead model
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Leads;

// This record holds the ID of the lead we want to find
public sealed record GetLeadByIdQuery(int LeadId);

// This handler fetches a single lead from the database by its ID
public sealed class GetLeadByIdHandler
{
    // The repository provides methods to read leads from the database
    private readonly ILeadRepository _repository;

    // Constructor: .NET injects the repository automatically
    public GetLeadByIdHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    // Look up a lead by its ID and return it (or null if not found)
    public Task<Lead?> HandleAsync(GetLeadByIdQuery request)
    {
        return Task.FromResult(_repository.GetLeadById(request.LeadId));
    }
}

/*
 * FILE SUMMARY:
 * This file contains the GetLeadByIdQuery (data) and GetLeadByIdHandler (logic) for fetching a single lead.
 * The query carries the lead's ID so the handler knows which lead to look up.
 * The handler asks the repository for the lead and returns it, or null if no lead has that ID.
 * This follows the CQRS pattern where queries represent read-only operations that don't change data.
 * It is called from the LeadsController when a GET request is made to /api/leads/{id}.
 */
