using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Data;
using LeadManagementSystem.Models;
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
    int? AssignedSalesRepId);

public sealed class UpdateLeadHandler
{
    private readonly ILeadRepository _repository;
    private readonly LeadDbContext _db;
    private readonly ILogger<UpdateLeadHandler> _logger;

    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        ["New"] = new() { "Contacted" },
        ["Contacted"] = new() { "Qualified", "Unqualified" },
        ["Qualified"] = new() { "Converted", "Unqualified" },
        ["Unqualified"] = new(),
        ["Converted"] = new(),
    };

    public UpdateLeadHandler(ILeadRepository repository, LeadDbContext db, ILogger<UpdateLeadHandler> logger)
    {
        _repository = repository;
        _db = db;
        _logger = logger;
    }

    public Task<OperationResult> HandleAsync(UpdateLeadCommand request)
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

        // Validation: AssignedSalesRepId must exist (must be a User with SalesRep role)
        if (request.AssignedSalesRepId.HasValue)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserId == request.AssignedSalesRepId.Value && u.Role == "SalesRep");
            if (user is null)
                return Task.FromResult(OperationResult.Fail("AssignedSalesRepId does not reference an existing sales rep user."));
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
            AssignedSalesRepId = request.AssignedSalesRepId,
            CreatedDate = existing.CreatedDate,
            ModifiedDate = DateTime.UtcNow,
            Interactions = existing.Interactions
        };

        _repository.UpdateLead(updatedLead);
        _logger.LogInformation("Lead updated: LeadId={LeadId}, Status={Status}", request.LeadId, request.Status);
        return Task.FromResult(OperationResult.Ok("Lead updated successfully."));
    }
}
