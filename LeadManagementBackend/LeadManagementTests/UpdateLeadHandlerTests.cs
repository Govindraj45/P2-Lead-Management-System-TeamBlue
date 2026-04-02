// Import shared result types used across all command handlers
using LeadManagementSystem.Features.Common;
// Import lead-specific command and handler classes
using LeadManagementSystem.Features.Leads;
// Import repository interfaces (contracts for data access)
using LeadManagementSystem.Interfaces;
// Import the Lead model (the main data object for leads)
using LeadManagementSystem.Models;
// Import logging interface for recording events
using Microsoft.Extensions.Logging;
// Import Moq library to create fake (mock) versions of dependencies
using Moq;

namespace LeadManagementTests;

// This class contains all tests for the UpdateLeadHandler (the code that edits existing leads)
public class UpdateLeadHandlerTests
{
    // Create fake versions of the lead repository, sales repository, and logger
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ISalesRepository> _salesRepo = new();
    private readonly Mock<ILogger<UpdateLeadHandler>> _logger = new();

    // Helper method that builds an UpdateLeadHandler using our fake dependencies
    private UpdateLeadHandler CreateHandler() => new(_leadRepo.Object, _salesRepo.Object, _logger.Object);

    // Helper method that creates a sample lead with a given status for use in tests
    private Lead CreateExistingLead(string status = "New") => new()
    {
        LeadId = 1,
        Name = "Existing",
        Email = "existing@test.com",
        Status = status,
        Source = "Website",
        Priority = "Medium",
        CreatedDate = DateTime.UtcNow.AddDays(-1)
    };

    // TEST: Updating a lead with valid data should succeed
    [Fact]
    public async Task UpdateLead_ValidInput_ReturnsSuccess()
    {
        // Set up a lead in the fake repo that we will update
        var existing = CreateExistingLead();
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(existing);
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead> { existing });
        var handler = CreateHandler();
        // Build a command that changes the name and priority
        var cmd = new UpdateLeadCommand(1, "Updated Name", "existing@test.com", null, null, null, "New", "Website", "High", null);

        var result = await handler.HandleAsync(cmd);

        // Should succeed and UpdateLead should be called once in the repo
        Assert.True(result.Success);
        _leadRepo.Verify(r => r.UpdateLead(It.IsAny<Lead>()), Times.Once);
    }

    // TEST: Trying to update a lead that does not exist should fail with "not found"
    [Fact]
    public async Task UpdateLead_NonExistentLead_ReturnsFail()
    {
        // Return null when looking up lead ID 99 (it does not exist)
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var handler = CreateHandler();
        var cmd = new UpdateLeadCommand(99, "Name", null, null, null, null, "New", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        // Should fail with a "not found" message
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    // TEST: Trying to update a lead that is already "Converted" should fail (converted = read-only)
    [Fact]
    public async Task UpdateLead_ConvertedLead_ReturnsFail()
    {
        // Create a lead with "Converted" status — it should be locked from edits
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(CreateExistingLead("Converted"));
        var handler = CreateHandler();
        var cmd = new UpdateLeadCommand(1, "Name", null, null, null, null, "Converted", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        // Should fail because converted leads cannot be modified
        Assert.False(result.Success);
        Assert.Contains("cannot be modified", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // TEST: Skipping a status step (New → Qualified) should fail; must follow the correct order
    [Fact]
    public async Task UpdateLead_InvalidStatusTransition_ReturnsFail()
    {
        var existing = CreateExistingLead("New");
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(existing);
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead> { existing });
        var handler = CreateHandler();
        // New → Qualified is not allowed; must go New → Contacted first
        var cmd = new UpdateLeadCommand(1, "Name", null, null, null, null, "Qualified", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        // Should fail because you cannot skip status steps
        Assert.False(result.Success);
        Assert.Contains("Cannot transition", result.Message);
    }

    // TEST: Moving a lead from New → Contacted (a valid next step) should succeed
    [Fact]
    public async Task UpdateLead_ValidStatusTransition_ReturnsSuccess()
    {
        var existing = CreateExistingLead("New");
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(existing);
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead> { existing });
        var handler = CreateHandler();
        // New → Contacted is a valid transition
        var cmd = new UpdateLeadCommand(1, "Name", null, null, null, null, "Contacted", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        // Should succeed because this status transition is allowed
        Assert.True(result.Success);
    }

    // TEST: Changing a lead's email to one that belongs to a different lead should fail
    [Fact]
    public async Task UpdateLead_DuplicateEmailDifferentLead_ReturnsFail()
    {
        // Set up two leads; lead2 already owns the email "taken@test.com"
        var lead1 = CreateExistingLead();
        var lead2 = new Lead { LeadId = 2, Email = "taken@test.com", Name = "Other", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead1);
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead> { lead1, lead2 });
        var handler = CreateHandler();
        // Try to change lead1's email to the email that lead2 already has
        var cmd = new UpdateLeadCommand(1, "Name", "taken@test.com", null, null, null, "New", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        // Should fail because this email is already used by another lead
        Assert.False(result.Success);
        Assert.Contains("email already exists", result.Message);
    }
}

/*
 * FILE SUMMARY:
 * This file tests the UpdateLeadHandler, which is responsible for editing existing leads.
 * It checks that valid updates succeed and that various invalid scenarios are caught: updating a
 * non-existent lead, editing a converted (read-only) lead, skipping status steps, and using a duplicate email.
 * Moq fakes are used to simulate the database so tests run fast and without external dependencies.
 * Each test expects either a success result or a specific error message explaining what went wrong.
 */
