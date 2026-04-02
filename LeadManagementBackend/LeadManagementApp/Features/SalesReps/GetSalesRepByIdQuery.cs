using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using MediatR;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record GetSalesRepByIdQuery(int RepId) : IRequest<SalesRep?>;

public sealed class GetSalesRepByIdHandler : IRequestHandler<GetSalesRepByIdQuery, SalesRep?>
{
    private readonly ISalesRepository _repository;

    public GetSalesRepByIdHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<SalesRep?> Handle(GetSalesRepByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_repository.GetRepById(request.RepId));
    }
}
