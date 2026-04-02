using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record DeleteLeadCommand(int LeadId) : IRequest<OperationResult>;

public sealed class DeleteLeadHandler : IRequestHandler<DeleteLeadCommand, OperationResult>
{
    private readonly ILeadRepository _repository;

    public DeleteLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult> Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var existing = _repository.GetLeadById(request.LeadId);
        if (existing is null)
        {
            return Task.FromResult(OperationResult.Fail("Lead not found."));
        }

        _repository.DeleteLead(request.LeadId);
        return Task.FromResult(OperationResult.Ok("Lead deleted successfully."));
    }
}
