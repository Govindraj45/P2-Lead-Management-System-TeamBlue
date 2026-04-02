using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record UpdateLeadStatusCommand(int LeadId, string NewStatus) : IRequest<OperationResult>;

public sealed class UpdateLeadStatusHandler : IRequestHandler<UpdateLeadStatusCommand, OperationResult>
{
    private readonly LeadService _leadService;

    public UpdateLeadStatusHandler(LeadService leadService)
    {
        _leadService = leadService;
    }

    public Task<OperationResult> Handle(UpdateLeadStatusCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_leadService.UpdateStatus(request.LeadId, request.NewStatus));
    }
}
