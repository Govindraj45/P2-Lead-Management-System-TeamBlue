using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record UpdateLeadStatusCommand(int LeadId, string NewStatus) : IRequest<OperationResult>;

public sealed class UpdateLeadStatusHandler : IRequestHandler<UpdateLeadStatusCommand, OperationResult>
{
    private readonly LeadService _leadService;
    private readonly ILogger<UpdateLeadStatusHandler> _logger;

    public UpdateLeadStatusHandler(LeadService leadService, ILogger<UpdateLeadStatusHandler> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    public Task<OperationResult> Handle(UpdateLeadStatusCommand request, CancellationToken cancellationToken)
    {
        var result = _leadService.UpdateStatus(request.LeadId, request.NewStatus);
        if (result.Success)
            _logger.LogInformation("Lead status changed: LeadId={LeadId}, NewStatus={Status}", request.LeadId, request.NewStatus);
        else
            _logger.LogWarning("Status change failed: LeadId={LeadId}, Reason={Reason}", request.LeadId, result.Message);
        return Task.FromResult(result);
    }
}
