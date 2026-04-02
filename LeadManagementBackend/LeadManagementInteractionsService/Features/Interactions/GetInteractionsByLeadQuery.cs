using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Interactions;

public sealed record GetInteractionsByLeadQuery(int LeadId);

public sealed class GetInteractionsByLeadHandler
{
    private readonly IInteractionRepository _repository;

    public GetInteractionsByLeadHandler(IInteractionRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Interaction>> HandleAsync(GetInteractionsByLeadQuery request)
    {
        return Task.FromResult(_repository.GetInteractionsByLead(request.LeadId));
    }
}
