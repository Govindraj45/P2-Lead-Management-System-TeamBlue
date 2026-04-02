// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Data;
using LeadManagementSystem.Features.Common;
using System.Text.RegularExpressions;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Leads;

// This is a Command = an action that changes data
// It holds all the information needed to create a new lead (like a form submission)
public sealed record CreateLeadCommand(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedSalesRepId);

// This is the handler — it contains the actual logic to process the command above
public sealed class CreateLeadHandler
{
    // These are the tools/services this handler needs to do its job
    // _repository talks to the database to save/read leads
    private readonly ILeadRepository _repository;
    // _db is the database connection itself (used for checking if a sales rep exists)
    private readonly LeadDbContext _db;
    // _logger writes messages to a log file so developers can track what happened
    private readonly ILogger<CreateLeadHandler> _logger;

    // This is the constructor — it receives the tools this handler needs when it's created
    public CreateLeadHandler(ILeadRepository repository, LeadDbContext db, ILogger<CreateLeadHandler> logger)
    {
        _repository = repository;
        _db = db;
        _logger = logger;
    }

    // This is the main method that runs when someone wants to create a new lead
    // It returns a result that tells you if it worked and includes the new lead's ID
    public Task<OperationResult<int>> HandleAsync(CreateLeadCommand request)
    {
        // Validation: Name is required — you can't create a lead without a name
        if (string.IsNullOrWhiteSpace(request.Name))
            return Task.FromResult(OperationResult<int>.Fail("Name is required."));

        // Validation: If an email was provided, make sure it looks like a real email
        if (!string.IsNullOrWhiteSpace(request.Email) && !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Task.FromResult(OperationResult<int>.Fail("Email must be a valid format."));

        // Validation: Check that no other lead already has this same email address
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingLeads = _repository.GetAllLeads();
            if (existingLeads.Any(l => string.Equals(l.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(OperationResult<int>.Fail("A lead with this email already exists."));
        }

        // Validation: If a phone number was provided, make sure it looks like a real phone number
        if (!string.IsNullOrWhiteSpace(request.Phone) && !Regex.IsMatch(request.Phone, @"^[\d\s\-\+\(\)]{7,20}$"))
            return Task.FromResult(OperationResult<int>.Fail("Phone must follow a valid format."));

        // Validation: If a sales rep was assigned, make sure that person actually exists in the database
        if (request.AssignedSalesRepId.HasValue)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserId == request.AssignedSalesRepId.Value && u.Role == "SalesRep");
            if (user is null)
                return Task.FromResult(OperationResult<int>.Fail("AssignedSalesRepId does not reference an existing sales rep user."));
        }

        // All validations passed — now create the actual lead object with the provided data
        // If Status, Source, or Priority were not provided, use default values
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
            AssignedSalesRepId = request.AssignedSalesRepId,
            CreatedDate = DateTime.UtcNow
        };

        // Save the new lead to the database
        _repository.AddLead(lead);
        // Write a log entry so developers can see a lead was created
        _logger.LogInformation("Lead created: LeadId={LeadId}, Name={Name}", lead.LeadId, lead.Name);
        // Return a success result with the new lead's ID
        return Task.FromResult(OperationResult<int>.Ok(lead.LeadId, "Lead created successfully."));
    }
}

/*
 * FILE SUMMARY: CreateLeadCommand.cs
 *
 * This file handles creating a brand new lead in the system (Command = action that changes data).
 * It first checks that all the input data is valid — name is required, email must be properly
 * formatted and unique, phone must be valid, and any assigned sales rep must actually exist.
 * If everything passes validation, it creates a new Lead object with default values for
 * missing fields and saves it to the database.
 * This is one of the most important files because it's the entry point for all new leads.
 */
