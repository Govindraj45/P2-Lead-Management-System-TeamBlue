// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Logic;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Leads;

// This is a Command = an action that changes data
// It holds the lead's ID and the new status you want to set
public sealed record UpdateLeadStatusCommand(int LeadId, string NewStatus);

// This is the handler — it contains the logic to change a lead's status
public sealed class UpdateLeadStatusHandler
{
    // _leadService contains the business logic for managing leads
    private readonly LeadService _leadService;
    // _logger writes messages to a log file for tracking
    private readonly ILogger<UpdateLeadStatusHandler> _logger;

    // The constructor receives the tools this handler needs when it's created
    public UpdateLeadStatusHandler(LeadService leadService, ILogger<UpdateLeadStatusHandler> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    // This method runs when someone wants to change a lead's status (e.g., "New" to "Contacted")
    public Task<OperationResult> HandleAsync(UpdateLeadStatusCommand request)
    {
        // Ask the lead service to perform the status update (it handles all the rules)
        var result = _leadService.UpdateStatus(request.LeadId, request.NewStatus);
        // If it worked, log a success message
        if (result.Success)
            _logger.LogInformation("Lead status changed: LeadId={LeadId}, NewStatus={Status}", request.LeadId, request.NewStatus);
        // If it failed, log a warning with the reason
        else
            _logger.LogWarning("Status change failed: LeadId={LeadId}, Reason={Reason}", request.LeadId, result.Message);
        // Return the result (success or failure) to the caller
        return Task.FromResult(result);
    }
}

/*
 * FILE SUMMARY: UpdateLeadStatusCommand.cs
 *
 * This file handles changing only the status of a lead (Command = action that changes data).
 * Unlike UpdateLeadCommand which updates everything, this focuses just on status changes
 * like moving a lead from "New" to "Contacted" or "Qualified" to "Converted".
 * It delegates the actual business rules to the LeadService and logs whether the change succeeded or failed.
 * This is useful when you just need a quick status update without changing other lead details.
 */
