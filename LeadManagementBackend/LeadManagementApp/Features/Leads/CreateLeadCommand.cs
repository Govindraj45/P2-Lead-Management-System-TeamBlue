using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;
using System.Text.RegularExpressions;

namespace LeadManagementSystem.Features.Leads;

public sealed record CreateLeadCommand(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedToRepId);

public sealed class CreateLeadHandler
{
    private readonly ILeadRepository _repository;
    private readonly ISalesRepository _salesRepository;
    private readonly ILogger<CreateLeadHandler> _logger;

    public CreateLeadHandler(ILeadRepository repository, ISalesRepository salesRepository, ILogger<CreateLeadHandler> logger)
    {
        _repository = repository;
        _salesRepository = salesRepository;
        _logger = logger;
    }

    public Task<OperationResult<int>> HandleAsync(CreateLeadCommand request)
    {
        // Validation: Name is required
        if (string.IsNullOrWhiteSpace(request.Name))
            return Task.FromResult(OperationResult<int>.Fail("Name is required."));

        // Validation: Email must be valid format
        if (!string.IsNullOrWhiteSpace(request.Email) && !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Task.FromResult(OperationResult<int>.Fail("Email must be a valid format."));

        // Validation: Email must be unique
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingLeads = _repository.GetAllLeads();
            if (existingLeads.Any(l => string.Equals(l.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(OperationResult<int>.Fail("A lead with this email already exists."));
        }

        // Validation: Phone must follow valid format
        if (!string.IsNullOrWhiteSpace(request.Phone) && !Regex.IsMatch(request.Phone, @"^[\d\s\-\+\(\)]{7,20}$"))
            return Task.FromResult(OperationResult<int>.Fail("Phone must follow a valid format."));

        // Validation: AssignedSalesRepId must exist
        if (request.AssignedToRepId.HasValue)
        {
            var rep = _salesRepository.GetRepById(request.AssignedToRepId.Value);
            if (rep is null)
                return Task.FromResult(OperationResult<int>.Fail("AssignedSalesRepId does not reference an existing sales rep."));
        }

        var lead = new Lead
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Position = request.Position,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "New" : request.Status,
            Source = string.IsNullOrWhiteSpace(request.Source) ? "Website" : request.Source,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority,
            AssignedToRepId = request.AssignedToRepId,
            CreatedDate = DateTime.UtcNow
        };

        _repository.AddLead(lead);
        _logger.LogInformation("Lead created: LeadId={LeadId}, Name={Name}", lead.LeadId, lead.Name);
        return Task.FromResult(OperationResult<int>.Ok(lead.LeadId, "Lead created successfully."));
    }
}
