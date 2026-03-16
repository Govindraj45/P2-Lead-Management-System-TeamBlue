using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using MediatR;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record GetAllSalesRepsQuery() : IRequest<List<SalesRep>>;

public sealed class GetAllSalesRepsHandler : IRequestHandler<GetAllSalesRepsQuery, List<SalesRep>>
{
    private readonly ISalesRepository _repository;

    public GetAllSalesRepsHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<List<SalesRep>> Handle(GetAllSalesRepsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_repository.GetAllReps());
    }
}
