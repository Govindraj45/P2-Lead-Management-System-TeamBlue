using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using MediatR;

namespace LeadManagementSystem.Features.Interactions;

public sealed record GetInteractionsByLeadQuery(int LeadId) : IRequest<List<Interaction>>;

public sealed class GetInteractionsByLeadHandler : IRequestHandler<GetInteractionsByLeadQuery, List<Interaction>>
{
    private readonly IInteractionRepository _repository;

    public GetInteractionsByLeadHandler(IInteractionRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Interaction>> Handle(GetInteractionsByLeadQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_repository.GetInteractionsByLead(request.LeadId));
    }
}
