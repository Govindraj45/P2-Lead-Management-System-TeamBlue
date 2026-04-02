// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Leads;

// This is a Command = an action that changes data
// It only needs the LeadId to know which lead to delete
public sealed record DeleteLeadCommand(int LeadId);

// This is the handler — it contains the logic to delete a lead
public sealed class DeleteLeadHandler
{
    // _repository talks to the database to read and delete leads
    private readonly ILeadRepository _repository;
    // _logger writes messages to a log file for tracking
    private readonly ILogger<DeleteLeadHandler> _logger;

    // The constructor receives the tools this handler needs when it's created
    public DeleteLeadHandler(ILeadRepository repository, ILogger<DeleteLeadHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // This method runs when someone wants to delete a lead
    public Task<OperationResult> HandleAsync(DeleteLeadCommand request)
    {
        // First, find the lead that needs to be deleted
        var existing = _repository.GetLeadById(request.LeadId);
        // If the lead doesn't exist, return an error
        if (existing is null)
            return Task.FromResult(OperationResult.Fail("Lead not found."));

        // Business rule: once a lead is "Converted" to a customer, it cannot be deleted
        if (existing.Status == "Converted")
            return Task.FromResult(OperationResult.Fail("Cannot delete a converted lead."));

        // All checks passed — delete the lead from the database
        _repository.DeleteLead(request.LeadId);
        // Write a log entry so developers can track that this lead was deleted
        _logger.LogInformation("Lead deleted: LeadId={LeadId}", request.LeadId);
        // Return a success result
        return Task.FromResult(OperationResult.Ok("Lead deleted successfully."));
    }
}

/*
 * FILE SUMMARY: DeleteLeadCommand.cs
 *
 * This file handles permanently removing a lead from the system (Command = action that changes data).
 * Before deleting, it checks two things: the lead must exist, and it must not be "Converted"
 * (because converted leads represent real customers and should never be deleted).
 * If both checks pass, the lead is removed from the database and the action is logged.
 * This protects important customer data while still allowing cleanup of unwanted leads.
 */
