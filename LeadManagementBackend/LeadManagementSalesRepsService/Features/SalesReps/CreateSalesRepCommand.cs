using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record CreateSalesRepCommand(
    string Name,
    string Email,
    string? Department);

public sealed class CreateSalesRepHandler
{
    private readonly ISalesRepository _repository;

    public CreateSalesRepHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult<int>> HandleAsync(CreateSalesRepCommand request)
    {
        var rep = new SalesRep
        {
            Name = request.Name,
            Email = request.Email,
            Department = string.IsNullOrWhiteSpace(request.Department) ? "Sales" : request.Department
        };

        _repository.AddSalesRep(rep);
        return Task.FromResult(OperationResult<int>.Ok(rep.RepId, "Sales representative created successfully."));
    }
}
