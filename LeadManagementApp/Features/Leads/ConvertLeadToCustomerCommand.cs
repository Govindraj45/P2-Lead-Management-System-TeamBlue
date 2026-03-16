using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record ConvertLeadToCustomerCommand(int LeadId) : IRequest<OperationResult>;

public sealed class ConvertLeadToCustomerHandler : IRequestHandler<ConvertLeadToCustomerCommand, OperationResult>
{
    private readonly LeadService _leadService;

    public ConvertLeadToCustomerHandler(LeadService leadService)
    {
        _leadService = leadService;
    }

    public Task<OperationResult> Handle(ConvertLeadToCustomerCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_leadService.ConvertToCustomer(request.LeadId));
    }
}
