using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;

namespace LeadManagementSystem.Features.Leads;

public sealed record DeleteLeadCommand(int LeadId);

public sealed class DeleteLeadHandler
{
    private readonly ILeadRepository _repository;
    private readonly ILogger<DeleteLeadHandler> _logger;

    public DeleteLeadHandler(ILeadRepository repository, ILogger<DeleteLeadHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<OperationResult> HandleAsync(DeleteLeadCommand request)
    {
        var existing = _repository.GetLeadById(request.LeadId);
        if (existing is null)
            return Task.FromResult(OperationResult.Fail("Lead not found."));

        if (existing.Status == "Converted")
            return Task.FromResult(OperationResult.Fail("Cannot delete a converted lead."));

        _repository.DeleteLead(request.LeadId);
        _logger.LogInformation("Lead deleted: LeadId={LeadId}", request.LeadId);
        return Task.FromResult(OperationResult.Ok("Lead deleted successfully."));
    }
}
