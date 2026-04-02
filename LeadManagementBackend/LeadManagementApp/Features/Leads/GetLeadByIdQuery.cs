using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Leads;

public sealed record GetLeadByIdQuery(int LeadId);

public sealed class GetLeadByIdHandler
{
    private readonly ILeadRepository _repository;

    public GetLeadByIdHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<Lead?> HandleAsync(GetLeadByIdQuery request)
    {
        return Task.FromResult(_repository.GetLeadById(request.LeadId));
    }
}
