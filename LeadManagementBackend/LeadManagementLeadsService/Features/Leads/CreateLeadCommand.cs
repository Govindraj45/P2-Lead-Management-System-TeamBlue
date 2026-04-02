using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Features.Leads;

public sealed record CreateLeadCommand(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedToRepId);

public sealed class CreateLeadHandler
{
    private readonly ILeadRepository _repository;

    public CreateLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult<int>> HandleAsync(CreateLeadCommand request)
    {
        var lead = new Lead
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "New" : request.Status,
            Source = string.IsNullOrWhiteSpace(request.Source) ? "Website" : request.Source,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority,
            AssignedToRepId = request.AssignedToRepId,
            CreatedDate = DateTime.UtcNow
        };

        _repository.AddLead(lead);
        return Task.FromResult(OperationResult<int>.Ok(lead.LeadId, "Lead created successfully."));
    }
}
