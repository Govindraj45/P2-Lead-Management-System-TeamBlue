using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;
using MediatR;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record CreateSalesRepCommand(
    string Name,
    string Email,
    string? Department) : IRequest<OperationResult<int>>;

public sealed class CreateSalesRepHandler : IRequestHandler<CreateSalesRepCommand, OperationResult<int>>
{
    private readonly ISalesRepository _repository;

    public CreateSalesRepHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult<int>> Handle(CreateSalesRepCommand request, CancellationToken cancellationToken)
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
