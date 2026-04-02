using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using MediatR;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record DeleteSalesRepCommand(int RepId) : IRequest<OperationResult>;

public sealed class DeleteSalesRepHandler : IRequestHandler<DeleteSalesRepCommand, OperationResult>
{
    private readonly ISalesRepository _repository;

    public DeleteSalesRepHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult> Handle(DeleteSalesRepCommand request, CancellationToken cancellationToken)
    {
        var existing = _repository.GetRepById(request.RepId);
        if (existing is null)
        {
            return Task.FromResult(OperationResult.Fail("Sales representative not found."));
        }

        _repository.DeleteRep(request.RepId);
        return Task.FromResult(OperationResult.Ok("Sales representative deleted successfully."));
    }
}
