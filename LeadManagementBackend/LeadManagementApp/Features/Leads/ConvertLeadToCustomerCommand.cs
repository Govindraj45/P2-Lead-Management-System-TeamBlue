// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Leads;

// This is a Command = an action that changes data
// It only needs the LeadId to know which lead to convert into a customer
public sealed record ConvertLeadToCustomerCommand(int LeadId);

// This is the handler — it contains the logic to convert a lead into a paying customer
public sealed class ConvertLeadToCustomerHandler
{
    // _leadService contains the business logic for managing leads
    private readonly LeadService _leadService;
    // _logger writes messages to a log file for tracking
    private readonly ILogger<ConvertLeadToCustomerHandler> _logger;

    // The constructor receives the tools this handler needs when it's created
    public ConvertLeadToCustomerHandler(LeadService leadService, ILogger<ConvertLeadToCustomerHandler> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    // This method runs when someone wants to convert a qualified lead into a customer
    // This is a big deal — only "Qualified" leads can be converted, and only Managers/Admins can do it
    public async Task<OperationResult> HandleAsync(ConvertLeadToCustomerCommand request)
    {
        // Ask the lead service to perform the conversion (it handles all the business rules)
        var result = await Task.FromResult(_leadService.ConvertToCustomer(request.LeadId));
        // If it worked, log a success message
        if (result.Success)
            _logger.LogInformation("Lead converted: LeadId={LeadId}", request.LeadId);
        // If it failed, log a warning with the reason
        else
            _logger.LogWarning("Lead conversion failed: LeadId={LeadId}, Reason={Reason}", request.LeadId, result.Message);
        // Return the result to the caller
        return result;
    }
}

/*
 * FILE SUMMARY: ConvertLeadToCustomerCommand.cs
 *
 * This file handles the most important business action: converting a lead into a real customer
 * (Command = action that changes data). This changes the lead's status to "Converted" permanently.
 * Only leads with "Qualified" status can be converted, and the actual conversion rules are
 * enforced by the LeadService. Once converted, the lead becomes read-only and cannot be edited or deleted.
 * This is a critical step in the sales pipeline — it represents a successful sale.
 */
