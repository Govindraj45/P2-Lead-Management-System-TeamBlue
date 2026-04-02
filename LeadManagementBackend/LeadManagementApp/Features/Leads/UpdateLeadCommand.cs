using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using MediatR;
using System.Text.RegularExpressions;

namespace LeadManagementSystem.Features.Leads;

public sealed record UpdateLeadCommand(
    int LeadId,
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string Status,
    string Source,
    string Priority,
    int? AssignedToRepId) : IRequest<OperationResult>;

public sealed class UpdateLeadHandler : IRequestHandler<UpdateLeadCommand, OperationResult>
{
    private readonly ILeadRepository _repository;
    private readonly ISalesRepository _salesRepository;
    private readonly ILogger<UpdateLeadHandler> _logger;

    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        ["New"] = new() { "New", "Contacted" },
        ["Contacted"] = new() { "Contacted", "Qualified", "Unqualified" },
        ["Qualified"] = new() { "Qualified", "Converted", "Unqualified" },
        ["Unqualified"] = new() { "Unqualified" },
        ["Converted"] = new() { "Converted" },
    };

    public UpdateLeadHandler(ILeadRepository repository, ISalesRepository salesRepository, ILogger<UpdateLeadHandler> logger)
    {
        _repository = repository;
        _salesRepository = salesRepository;
        _logger = logger;
    }

    public Task<OperationResult> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var existing = _repository.GetLeadById(request.LeadId);
        if (existing is null)
            return Task.FromResult(OperationResult.Fail("Lead not found."));

        if (existing.Status == "Converted")
            return Task.FromResult(OperationResult.Fail("Converted leads cannot be modified."));

        // Validation: Name is required
        if (string.IsNullOrWhiteSpace(request.Name))
            return Task.FromResult(OperationResult.Fail("Name is required."));

        // Validation: Email format
        if (!string.IsNullOrWhiteSpace(request.Email) && !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Task.FromResult(OperationResult.Fail("Email must be a valid format."));

        // Validation: Email uniqueness (exclude self)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var allLeads = _repository.GetAllLeads();
            if (allLeads.Any(l => l.LeadId != request.LeadId && string.Equals(l.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(OperationResult.Fail("A lead with this email already exists."));
        }

        // Validation: Phone format
        if (!string.IsNullOrWhiteSpace(request.Phone) && !Regex.IsMatch(request.Phone, @"^[\d\s\-\+\(\)]{7,20}$"))
            return Task.FromResult(OperationResult.Fail("Phone must follow a valid format."));

        // Validation: Status transition
        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != existing.Status)
        {
            if (!AllowedTransitions.TryGetValue(existing.Status, out var allowed) || !allowed.Contains(request.Status))
                return Task.FromResult(OperationResult.Fail($"Cannot transition from {existing.Status} to {request.Status}."));
        }

        // Validation: AssignedSalesRepId must exist
        if (request.AssignedToRepId.HasValue)
        {
            var rep = _salesRepository.GetRepById(request.AssignedToRepId.Value);
            if (rep is null)
                return Task.FromResult(OperationResult.Fail("AssignedSalesRepId does not reference an existing sales rep."));
        }

        var updatedLead = new Lead
        {
            LeadId = existing.LeadId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Position = request.Position,
            Status = request.Status,
            Source = request.Source,
            Priority = request.Priority,
            AssignedToRepId = request.AssignedToRepId,
            CreatedDate = existing.CreatedDate,
            ModifiedDate = DateTime.UtcNow,
            Interactions = existing.Interactions
        };

        _repository.UpdateLead(updatedLead);
        _logger.LogInformation("Lead updated: LeadId={LeadId}, Status={Status}", request.LeadId, request.Status);
        return Task.FromResult(OperationResult.Ok("Lead updated successfully."));
    }
}
