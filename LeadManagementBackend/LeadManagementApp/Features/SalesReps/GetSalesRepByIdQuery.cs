using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record GetSalesRepByIdQuery(int RepId);

public sealed class GetSalesRepByIdHandler
{
    private readonly ISalesRepository _repository;

    public GetSalesRepByIdHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<SalesRep?> HandleAsync(GetSalesRepByIdQuery request)
    {
        return Task.FromResult(_repository.GetRepById(request.RepId));
    }
}
