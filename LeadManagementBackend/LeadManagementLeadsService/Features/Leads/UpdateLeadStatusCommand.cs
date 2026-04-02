using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;

namespace LeadManagementSystem.Features.Leads;

public sealed record UpdateLeadStatusCommand(int LeadId, string NewStatus);

public sealed class UpdateLeadStatusHandler
{
    private readonly LeadService _leadService;

    public UpdateLeadStatusHandler(LeadService leadService)
    {
        _leadService = leadService;
    }

    public Task<OperationResult> HandleAsync(UpdateLeadStatusCommand request)
    {
        return Task.FromResult(_leadService.UpdateStatus(request.LeadId, request.NewStatus));
    }
}
