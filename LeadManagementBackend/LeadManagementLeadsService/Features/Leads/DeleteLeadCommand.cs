using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;

namespace LeadManagementSystem.Features.Leads;

public sealed record DeleteLeadCommand(int LeadId);

public sealed class DeleteLeadHandler
{
    private readonly ILeadRepository _repository;

    public DeleteLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult> HandleAsync(DeleteLeadCommand request)
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
