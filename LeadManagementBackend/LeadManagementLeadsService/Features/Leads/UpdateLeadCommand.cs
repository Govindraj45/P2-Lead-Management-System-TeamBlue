using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.Leads;

public sealed record UpdateLeadCommand(
    int LeadId,
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string Status,
    string Source,
    string Priority,
    int? AssignedToRepId);

public sealed class UpdateLeadHandler
{
    private readonly ILeadRepository _repository;

    public UpdateLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult> HandleAsync(UpdateLeadCommand request)
    {
        var existing = _repository.GetLeadById(request.LeadId);
        if (existing is null)
        {
            return Task.FromResult(OperationResult.Fail("Lead not found."));
        }

        var updatedLead = new Lead
        {
            LeadId = request.LeadId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Status = request.Status,
            Source = request.Source,
            Priority = request.Priority,
            AssignedToRepId = request.AssignedToRepId,
            CreatedDate = existing.CreatedDate,
            Interactions = existing.Interactions
        };

        _repository.UpdateLead(updatedLead);
        return Task.FromResult(OperationResult.Ok("Lead updated successfully."));
    }
}
