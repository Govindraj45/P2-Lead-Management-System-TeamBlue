using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record GetAllLeadsQuery() : IRequest<List<Lead>>;

public sealed class GetAllLeadsHandler : IRequestHandler<GetAllLeadsQuery, List<Lead>>
{
    private readonly ILeadRepository _repository;

    public GetAllLeadsHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Lead>> Handle(GetAllLeadsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_repository.GetAllLeads());
    }
}
