using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;

namespace LeadManagementSystem.Features.Leads;

public sealed record ConvertLeadToCustomerCommand(int LeadId);

public sealed class ConvertLeadToCustomerHandler
{
    private readonly LeadService _leadService;
    private readonly ILogger<ConvertLeadToCustomerHandler> _logger;

    public ConvertLeadToCustomerHandler(LeadService leadService, ILogger<ConvertLeadToCustomerHandler> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    public async Task<OperationResult> HandleAsync(ConvertLeadToCustomerCommand request)
    {
        var result = await Task.FromResult(_leadService.ConvertToCustomer(request.LeadId));
        if (result.Success)
            _logger.LogInformation("Lead converted: LeadId={LeadId}", request.LeadId);
        else
            _logger.LogWarning("Lead conversion failed: LeadId={LeadId}, Reason={Reason}", request.LeadId, result.Message);
        return result;
    }
}
