using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;
using MediatR;

namespace LeadManagementSystem.Features.Leads;

public sealed record ConvertLeadToCustomerCommand(int LeadId) : IRequest<OperationResult>;

public sealed class ConvertLeadToCustomerHandler : IRequestHandler<ConvertLeadToCustomerCommand, OperationResult>
{
    private readonly LeadService _leadService;
    private readonly ILogger<ConvertLeadToCustomerHandler> _logger;

    public ConvertLeadToCustomerHandler(LeadService leadService, ILogger<ConvertLeadToCustomerHandler> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    public async Task<OperationResult> Handle(ConvertLeadToCustomerCommand request, CancellationToken cancellationToken)
    {
        var result = await Task.FromResult(_leadService.ConvertToCustomer(request.LeadId));
        if (result.Success)
            _logger.LogInformation("Lead converted: LeadId={LeadId}", request.LeadId);
        else
            _logger.LogWarning("Lead conversion failed: LeadId={LeadId}, Reason={Reason}", request.LeadId, result.Message);
        return result;
    }
}
