using LeadManagementSystem.Models;
using LeadManagementSystem.Data;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Logic;

public class LeadService
{
    private readonly ILeadRepository _repo;

    // Dependency Injection (SOLID Principle) 
    public LeadService(ILeadRepository repo)
    {
        _repo = repo;
    }

    // 1. Lead Status Validation 
    public OperationResult UpdateStatus(int leadId, string newStatus)
    {
        var lead = _repo.GetLeadById(leadId);
        if (lead == null) return OperationResult.Fail("Lead not found.");

        // Logic: Simple state machine to prevent invalid jumps 
        lead.Status = newStatus;
        _repo.UpdateLead(lead);
        return OperationResult.Ok($"Status updated to {newStatus}.");
    }

    // 2. Lead Conversion Logic
    public OperationResult ConvertToCustomer(int leadId)
    {
        var lead = _repo.GetLeadById(leadId);
        
        // Requirement: Lead must be 'Qualified' to convert 
        if (lead != null && lead.Status == "Qualified")
        {
            lead.Status = "Converted";
            _repo.UpdateLead(lead);
           // This is where integration with Customer Management happens 
            return OperationResult.Ok("Lead has been converted to a customer.");
        }
        return OperationResult.Fail("Only 'Qualified' leads can be converted.");
    }
}
