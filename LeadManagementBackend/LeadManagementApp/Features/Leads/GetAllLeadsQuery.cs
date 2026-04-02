using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Leads;

public sealed record GetAllLeadsQuery();

public sealed class GetAllLeadsHandler
{
    private readonly ILeadRepository _repository;

    public GetAllLeadsHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Lead>> HandleAsync(GetAllLeadsQuery request)
    {
        return Task.FromResult(_repository.GetAllLeads());
    }
}
