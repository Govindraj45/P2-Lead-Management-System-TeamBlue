// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Data;
using LeadManagementSystem.Models;
using System.Text.RegularExpressions;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Leads;

// This is a Command = an action that changes data
// It holds all the updated information for an existing lead
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

// This is the handler — it contains the actual logic to update a lead
public sealed class UpdateLeadHandler
{
    // These are the tools/services this handler needs to do its job
    private readonly ILeadRepository _repository;
    private readonly LeadDbContext _db;
    private readonly ILogger<UpdateLeadHandler> _logger;

    // This dictionary defines the rules for how a lead's status can change
    // For example: a "New" lead can only move to "Contacted", not skip ahead to "Qualified"
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        ["New"] = new() { "Contacted" },
        ["Contacted"] = new() { "Qualified", "Unqualified" },
        ["Qualified"] = new() { "Converted", "Unqualified" },
        ["Unqualified"] = new(),
        ["Converted"] = new(),
    };

    // The constructor receives the tools this handler needs when it's created
    public UpdateLeadHandler(ILeadRepository repository, LeadDbContext db, ILogger<UpdateLeadHandler> logger)
    {
        _repository = repository;
        _db = db;
        _logger = logger;
    }

    // This is the main method that runs when someone wants to update a lead
    public Task<OperationResult> HandleAsync(UpdateLeadCommand request)
    {
        // First, find the lead that needs to be updated
        var existing = _repository.GetLeadById(request.LeadId);
        // If the lead doesn't exist, return an error
        if (existing is null)
            return Task.FromResult(OperationResult.Fail("Lead not found."));

        // Business rule: once a lead is "Converted" to a customer, it cannot be changed
        if (existing.Status == "Converted")
            return Task.FromResult(OperationResult.Fail("Converted leads cannot be modified."));

        // Validation: Name is required — you can't have a lead without a name
        if (string.IsNullOrWhiteSpace(request.Name))
            return Task.FromResult(OperationResult.Fail("Name is required."));

        // Validation: If an email was provided, make sure it looks like a real email
        if (!string.IsNullOrWhiteSpace(request.Email) && !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Task.FromResult(OperationResult.Fail("Email must be a valid format."));

        // Validation: Make sure no OTHER lead already has this email (exclude the one being updated)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var allLeads = _repository.GetAllLeads();
            if (allLeads.Any(l => l.LeadId != request.LeadId && string.Equals(l.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(OperationResult.Fail("A lead with this email already exists."));
        }

        // Validation: If a phone number was provided, make sure it looks valid
        if (!string.IsNullOrWhiteSpace(request.Phone) && !Regex.IsMatch(request.Phone, @"^[\d\s\-\+\(\)]{7,20}$"))
            return Task.FromResult(OperationResult.Fail("Phone must follow a valid format."));

        // Validation: Check that the status change follows the allowed rules
        // For example, you can't jump from "New" directly to "Converted"
        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != existing.Status)
        {
            if (!AllowedTransitions.TryGetValue(existing.Status, out var allowed) || !allowed.Contains(request.Status))
                return Task.FromResult(OperationResult.Fail($"Cannot transition from {existing.Status} to {request.Status}."));
        }

        // Validation: If a sales rep was assigned, make sure that person actually exists
        if (request.AssignedSalesRepId.HasValue)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserId == request.AssignedSalesRepId.Value && u.Role == "SalesRep");
            if (user is null)
                return Task.FromResult(OperationResult.Fail("AssignedSalesRepId does not reference an existing sales rep user."));
        }

        // All validations passed — build the updated lead object with the new data
        // Keep the original creation date and interactions, but update the modified date
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

        // Save the updated lead to the database
        _repository.UpdateLead(updatedLead);
        // Write a log entry so developers can track what was changed
        _logger.LogInformation("Lead updated: LeadId={LeadId}, Status={Status}", request.LeadId, request.Status);
        // Return a success result
        return Task.FromResult(OperationResult.Ok("Lead updated successfully."));
    }
}

/*
 * FILE SUMMARY: UpdateLeadCommand.cs
 *
 * This file handles updating an existing lead's information (Command = action that changes data).
 * It performs many validations: the lead must exist, converted leads can't be edited, name is required,
 * email must be valid and unique, phone must be valid, and status changes must follow strict rules
 * (e.g., "New" can only go to "Contacted", not skip to "Qualified").
 * This is a critical file because it enforces the business rules that protect data integrity.
 */
