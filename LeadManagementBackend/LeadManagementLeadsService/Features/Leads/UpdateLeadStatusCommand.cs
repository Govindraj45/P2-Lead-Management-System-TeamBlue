// Import the OperationResult type for returning success/failure responses
using LeadManagementSystem.Features.Common;
// Import the LeadService which contains business rules for status transitions
using LeadManagementSystem.Logic;

namespace LeadManagementSystem.Features.Leads;

// This record holds the lead's ID and the new status to apply (e.g., "Contacted", "Qualified")
public sealed record UpdateLeadStatusCommand(int LeadId, string NewStatus);

// This handler updates only the status of a lead, using business rules from LeadService
public sealed class UpdateLeadStatusHandler
{
    // The LeadService enforces valid status transitions (e.g., can't skip from New to Converted)
    private readonly LeadService _leadService;

    // Constructor: .NET injects the service automatically
    public UpdateLeadStatusHandler(LeadService leadService)
    {
        _leadService = leadService;
    }

    // Delegate the status update to the LeadService, which validates the transition
    public Task<OperationResult> HandleAsync(UpdateLeadStatusCommand request)
    {
        return Task.FromResult(_leadService.UpdateStatus(request.LeadId, request.NewStatus));
    }
}

/*
 * FILE SUMMARY:
 * This file contains the UpdateLeadStatusCommand (data) and UpdateLeadStatusHandler (logic) for changing a lead's status.
 * Unlike a full update, this only changes the status field (e.g., from "New" to "Contacted").
 * The handler delegates to LeadService, which enforces business rules about valid status transitions.
 * This follows the CQRS pattern where commands represent actions that change data.
 * It is called from the LeadsController when a PUT request is made to /api/leads/{id}/status.
 */
