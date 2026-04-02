using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record GetAllSalesRepsQuery();

public sealed class GetAllSalesRepsHandler
{
    private readonly ISalesRepository _repository;

    public GetAllSalesRepsHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<List<SalesRep>> HandleAsync(GetAllSalesRepsQuery request)
    {
        return Task.FromResult(_repository.GetAllReps());
    }
}
