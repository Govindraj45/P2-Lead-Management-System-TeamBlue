// Import shared result types used across all command handlers
using LeadManagementSystem.Features.Common;
// Import lead-specific command and handler classes
using LeadManagementSystem.Features.Leads;
// Import repository interfaces (contracts for data access)
using LeadManagementSystem.Interfaces;
// Import the LeadService which contains business rules for conversion
using LeadManagementSystem.Logic;
// Import the Lead model
using LeadManagementSystem.Models;
// Import logging interface
using Microsoft.Extensions.Logging;
// Import Moq for creating fake dependencies
using Moq;

namespace LeadManagementTests;

// This class tests the ConvertLeadToCustomerHandler (the code that converts a lead into a customer)
public class ConvertLeadHandlerTests
{
    // Create fake versions of the lead repository and logger
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<ConvertLeadToCustomerHandler>> _logger = new();

    // TEST: Converting a "Qualified" lead should succeed (only Qualified leads can be converted)
    [Fact]
    public async Task ConvertLead_QualifiedLead_ReturnsSuccess()
    {
        // Create a lead with status "Qualified" — the only status that allows conversion
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "Qualified" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        // LeadService contains the rule: only "Qualified" leads can be converted
        var service = new LeadService(_leadRepo.Object);
        var handler = new ConvertLeadToCustomerHandler(service, _logger.Object);

        // Try to convert the lead
        var result = await handler.HandleAsync(new ConvertLeadToCustomerCommand(1));

        // Should succeed and the lead's status should now be "Converted"
        Assert.True(result.Success);
        _leadRepo.Verify(r => r.UpdateLead(It.Is<Lead>(l => l.Status == "Converted")), Times.Once);
    }

    // TEST: Converting a lead that is NOT "Qualified" (e.g., "New") should fail
    [Fact]
    public async Task ConvertLead_NonQualifiedLead_ReturnsFail()
    {
        // Create a lead with status "New" — not ready for conversion
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new ConvertLeadToCustomerHandler(service, _logger.Object);

        // Try to convert a "New" lead — this should not be allowed
        var result = await handler.HandleAsync(new ConvertLeadToCustomerCommand(1));

        // Should fail because only "Qualified" leads can be converted
        Assert.False(result.Success);
    }

    // TEST: Converting a lead that does not exist should fail
    [Fact]
    public async Task ConvertLead_NonExistentLead_ReturnsFail()
    {
        // Return null for lead ID 99 (it does not exist in the system)
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var service = new LeadService(_leadRepo.Object);
        var handler = new ConvertLeadToCustomerHandler(service, _logger.Object);

        // Try to convert a non-existent lead
        var result = await handler.HandleAsync(new ConvertLeadToCustomerCommand(99));

        // Should fail because the lead was not found
        Assert.False(result.Success);
    }
}

/*
 * FILE SUMMARY:
 * This file tests the ConvertLeadToCustomerHandler, which turns a lead into a paying customer.
 * The key business rule is: only leads with status "Qualified" can be converted to "Converted".
 * These tests verify that qualified leads convert successfully, non-qualified leads are rejected,
 * and non-existent leads return a failure. Conversion is a one-way action — once converted,
 * a lead becomes read-only and cannot be changed or deleted.
 */
