// Import the OperationResult type for returning success/failure responses
using LeadManagementSystem.Features.Common;
// Import the LeadService which contains business rules for lead conversion
using LeadManagementSystem.Logic;

namespace LeadManagementSystem.Features.Leads;

// This record holds the ID of the lead to convert into a customer
public sealed record ConvertLeadToCustomerCommand(int LeadId);

// This handler converts a qualified lead into a customer using business rules from LeadService
public sealed class ConvertLeadToCustomerHandler
{
    // The LeadService enforces rules like: only "Qualified" leads can be converted
    private readonly LeadService _leadService;

    // Constructor: .NET injects the service automatically
    public ConvertLeadToCustomerHandler(LeadService leadService)
    {
        _leadService = leadService;
    }

    // Delegate the conversion to the LeadService, which validates the lead's current status
    public Task<OperationResult> HandleAsync(ConvertLeadToCustomerCommand request)
    {
        return Task.FromResult(_leadService.ConvertToCustomer(request.LeadId));
    }
}

/*
 * FILE SUMMARY:
 * This file contains the ConvertLeadToCustomerCommand (data) and ConvertLeadToCustomerHandler (logic) for lead conversion.
 * Converting a lead means changing it from a "Qualified" lead into a "Converted" customer.
 * The handler delegates to LeadService, which enforces the rule that only qualified leads can be converted.
 * This follows the CQRS pattern where commands represent actions that change data.
 * It is called from the LeadsController when a POST request is made to /api/leads/{id}/convert.
 */
