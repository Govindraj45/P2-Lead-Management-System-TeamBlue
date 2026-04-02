using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record GetLeadByIdQuery(int LeadId) : IRequest<Lead?>;

public sealed class GetLeadByIdHandler : IRequestHandler<GetLeadByIdQuery, Lead?>
{
    private readonly ILeadRepository _repository;

    public GetLeadByIdHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<Lead?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_repository.GetLeadById(request.LeadId));
    }
}
