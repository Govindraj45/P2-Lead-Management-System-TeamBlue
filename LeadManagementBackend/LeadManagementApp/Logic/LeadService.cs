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
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        ["New"] = new() { "Contacted" },
        ["Contacted"] = new() { "Qualified", "Unqualified" },
        ["Qualified"] = new() { "Converted", "Unqualified" },
        ["Unqualified"] = new(),
        ["Converted"] = new(),
    };

    public OperationResult UpdateStatus(int leadId, string newStatus)
    {
        var lead = _repo.GetLeadById(leadId);
        if (lead == null) return OperationResult.Fail("Lead not found.");
        if (lead.Status == "Converted") return OperationResult.Fail("Converted leads cannot be modified.");

        if (!AllowedTransitions.TryGetValue(lead.Status, out var allowed) || !allowed.Contains(newStatus))
        {
            return OperationResult.Fail($"Cannot transition from {lead.Status} to {newStatus}.");
        }

        lead.Status = newStatus;
        lead.ModifiedDate = DateTime.UtcNow;
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
            lead.ConvertedDate = DateTime.UtcNow;
            lead.ModifiedDate = DateTime.UtcNow;
            _repo.UpdateLead(lead);
           // This is where integration with Customer Management happens 
            return OperationResult.Ok("Lead has been converted to a customer.");
        }
        return OperationResult.Fail("Only 'Qualified' leads can be converted.");
    }
}
