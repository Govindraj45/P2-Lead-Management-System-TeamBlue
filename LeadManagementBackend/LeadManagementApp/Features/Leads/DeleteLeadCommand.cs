using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record DeleteLeadCommand(int LeadId) : IRequest<OperationResult>;

public sealed class DeleteLeadHandler : IRequestHandler<DeleteLeadCommand, OperationResult>
{
    private readonly ILeadRepository _repository;
    private readonly ILogger<DeleteLeadHandler> _logger;

    public DeleteLeadHandler(ILeadRepository repository, ILogger<DeleteLeadHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<OperationResult> Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
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
