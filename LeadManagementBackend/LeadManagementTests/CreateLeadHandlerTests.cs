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

// This namespace groups all test classes together
namespace LeadManagementTests;

// This class contains all tests for the CreateLeadHandler (the code that creates new leads)
public class CreateLeadHandlerTests
{
    // Create fake versions of the lead repository, sales repository, and logger
    // These fakes let us control what data is returned without needing a real database
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ISalesRepository> _salesRepo = new();
    private readonly Mock<ILogger<CreateLeadHandler>> _logger = new();

    // Helper method that builds a CreateLeadHandler using our fake dependencies
    private CreateLeadHandler CreateHandler() => new(_leadRepo.Object, _salesRepo.Object, _logger.Object);

    // TEST: Creating a lead with all valid fields should succeed
    [Fact]
    public async Task CreateLead_ValidInput_ReturnsSuccess()
    {
        // Set up the fake repo to return an empty list (no existing leads)
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var handler = CreateHandler();
        // Build a command with valid fields: name, email, phone, company, etc.
        var cmd = new CreateLeadCommand("John Doe", "john@test.com", "1234567890", "Acme", "Dev", "New", "Website", "High", null);

        // Run the handler and get the result
        var result = await handler.HandleAsync(cmd);

        // The result should be successful and AddLead should have been called once
        Assert.True(result.Success);
        _leadRepo.Verify(r => r.AddLead(It.IsAny<Lead>()), Times.Once);
    }

    // TEST: Creating a lead with an empty name should fail with "Name is required"
    [Fact]
    public async Task CreateLead_EmptyName_ReturnsFail()
    {
        var handler = CreateHandler();
        // Name is empty string "" — this should be rejected by validation
        var cmd = new CreateLeadCommand("", "john@test.com", null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        // Expect failure with an error message about the name being required
        Assert.False(result.Success);
        Assert.Contains("Name is required", result.Message);
    }

    // TEST: Creating a lead with a badly formatted email should fail
    [Fact]
    public async Task CreateLead_InvalidEmail_ReturnsFail()
    {
        var handler = CreateHandler();
        // "not-an-email" is not a valid email address
        var cmd = new CreateLeadCommand("John", "not-an-email", null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        // Expect failure with an email format error
        Assert.False(result.Success);
        Assert.Contains("Email must be a valid format", result.Message);
    }

    // TEST: Creating a lead with an email that already exists should fail
    [Fact]
    public async Task CreateLead_DuplicateEmail_ReturnsFail()
    {
        // Set up fake repo so it already has a lead with email "john@test.com"
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new Lead { LeadId = 1, Email = "john@test.com", Name = "Existing" }
        });
        var handler = CreateHandler();
        // Try to create another lead with the same email
        var cmd = new CreateLeadCommand("John", "john@test.com", null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        // Should fail because the email is already taken
        Assert.False(result.Success);
        Assert.Contains("email already exists", result.Message);
    }

    // TEST: Creating a lead with an invalid phone number should fail
    [Fact]
    public async Task CreateLead_InvalidPhone_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var handler = CreateHandler();
        // "abc" is not a valid phone number
        var cmd = new CreateLeadCommand("John", null, "abc", null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        // Expect failure mentioning phone format
        Assert.False(result.Success);
        Assert.Contains("Phone must follow a valid format", result.Message);
    }

    // TEST: Assigning a lead to a sales rep that does not exist should fail
    [Fact]
    public async Task CreateLead_NonExistentSalesRep_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        // Return null when looking up sales rep ID 999 (does not exist)
        _salesRepo.Setup(r => r.GetRepById(It.IsAny<int>())).Returns((SalesRep?)null);
        var handler = CreateHandler();
        // Try assigning to rep ID 999 which does not exist
        var cmd = new CreateLeadCommand("John", null, null, null, null, null, null, null, 999);

        var result = await handler.HandleAsync(cmd);

        // Should fail because the sales rep does not exist
        Assert.False(result.Success);
        Assert.Contains("does not reference an existing sales rep", result.Message);
    }

    // TEST: Creating a lead with only a name (all other fields null) should succeed
    [Fact]
    public async Task CreateLead_NullOptionalFields_ReturnsSuccess()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var handler = CreateHandler();
        // Only the name is provided; everything else is optional and set to null
        var cmd = new CreateLeadCommand("Jane", null, null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        // Should succeed with a confirmation message
        Assert.True(result.Success);
        Assert.Contains("created successfully", result.Message);
    }
}

/*
 * FILE SUMMARY:
 * This file tests the CreateLeadHandler, which is responsible for creating new leads in the system.
 * It verifies that valid leads are created successfully and that invalid inputs (empty name, bad email,
 * duplicate email, invalid phone, non-existent sales rep) are properly rejected with clear error messages.
 * These tests use Moq to create fake repositories so no real database is needed during testing.
 * The expected outcome for each test is either a success or a specific failure message depending on the input.
 */
