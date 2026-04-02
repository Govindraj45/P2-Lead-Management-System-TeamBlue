// Import shared result types used across all command handlers
using LeadManagementSystem.Features.Common;
// Import lead-specific command and handler classes
using LeadManagementSystem.Features.Leads;
// Import repository interfaces (contracts for data access)
using LeadManagementSystem.Interfaces;
// Import the LeadService which contains status transition business logic
using LeadManagementSystem.Logic;
// Import the Lead model
using LeadManagementSystem.Models;
// Import logging interface
using Microsoft.Extensions.Logging;
// Import Moq for creating fake dependencies
using Moq;

namespace LeadManagementTests;

// This class tests the UpdateLeadStatusHandler, which only changes a lead's status (not other fields)
public class UpdateLeadStatusHandlerTests
{
    // Create fake versions of the lead repository and logger
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<UpdateLeadStatusHandler>> _logger = new();

    // TEST: Changing status from New → Contacted (a valid step) should succeed
    [Fact]
    public async Task UpdateStatus_ValidTransition_ReturnsSuccess()
    {
        // Create a lead with status "New"
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        // LeadService contains the real business rules for status transitions
        var service = new LeadService(_leadRepo.Object);
        var handler = new UpdateLeadStatusHandler(service, _logger.Object);

        // Try to move from New → Contacted
        var result = await handler.HandleAsync(new UpdateLeadStatusCommand(1, "Contacted"));

        // Should succeed because New → Contacted is allowed
        Assert.True(result.Success);
    }

    // TEST: Jumping from New → Converted (skipping steps) should fail
    [Fact]
    public async Task UpdateStatus_InvalidTransition_ReturnsFail()
    {
        // Create a lead with status "New"
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new UpdateLeadStatusHandler(service, _logger.Object);

        // Try to jump directly from New → Converted (not allowed)
        var result = await handler.HandleAsync(new UpdateLeadStatusCommand(1, "Converted"));

        // Should fail because you cannot skip status steps
        Assert.False(result.Success);
        Assert.Contains("Cannot transition", result.Message);
    }

    // TEST: A lead that is already "Converted" cannot have its status changed at all
    [Fact]
    public async Task UpdateStatus_ConvertedLead_ReturnsFail()
    {
        // Create a lead that is already converted (read-only)
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "Converted" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new UpdateLeadStatusHandler(service, _logger.Object);

        // Try to change a converted lead back to "New"
        var result = await handler.HandleAsync(new UpdateLeadStatusCommand(1, "New"));

        // Should fail because converted leads are locked
        Assert.False(result.Success);
    }
}

/*
 * FILE SUMMARY:
 * This file tests the UpdateLeadStatusHandler, which handles changing only the status of a lead.
 * It verifies that valid status transitions (like New → Contacted) succeed, and that invalid
 * transitions (like New → Converted, skipping steps) are rejected by the business rules.
 * It also confirms that once a lead is converted, its status can never be changed again.
 * The LeadService contains the real transition logic and is tested through the handler here.
 */
