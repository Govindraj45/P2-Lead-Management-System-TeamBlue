using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;

namespace LeadManagementSystem.Features.Leads;

public sealed record ConvertLeadToCustomerCommand(int LeadId);

public sealed class ConvertLeadToCustomerHandler
{
    private readonly LeadService _leadService;

    public ConvertLeadToCustomerHandler(LeadService leadService)
    {
        _leadService = leadService;
    }

    public Task<OperationResult> HandleAsync(ConvertLeadToCustomerCommand request)
    {
        return Task.FromResult(_leadService.ConvertToCustomer(request.LeadId));
    }
}
