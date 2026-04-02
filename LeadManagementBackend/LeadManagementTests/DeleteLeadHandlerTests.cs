// Import shared result types used across all command handlers
using LeadManagementSystem.Features.Common;
// Import lead-specific command and handler classes
using LeadManagementSystem.Features.Leads;
// Import repository interfaces (contracts for data access)
using LeadManagementSystem.Interfaces;
// Import the Lead model
using LeadManagementSystem.Models;
// Import logging interface
using Microsoft.Extensions.Logging;
// Import Moq for creating fake dependencies
using Moq;

namespace LeadManagementTests;

// This class tests the DeleteLeadHandler, which handles removing leads from the system
public class DeleteLeadHandlerTests
{
    // Create fake versions of the lead repository and logger
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<DeleteLeadHandler>> _logger = new();

    // Helper method that builds a DeleteLeadHandler with our fake dependencies
    private DeleteLeadHandler CreateHandler() => new(_leadRepo.Object, _logger.Object);

    // TEST: Deleting a lead that exists and is not converted should succeed
    [Fact]
    public async Task DeleteLead_ExistingLead_ReturnsSuccess()
    {
        // Set up a lead with status "New" — it is allowed to be deleted
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();

        // Try to delete lead with ID 1
        var result = await handler.HandleAsync(new DeleteLeadCommand(1));

        // Should succeed and DeleteLead should be called exactly once
        Assert.True(result.Success);
        _leadRepo.Verify(r => r.DeleteLead(1), Times.Once);
    }

    // TEST: Deleting a lead that does not exist should fail with "not found"
    [Fact]
    public async Task DeleteLead_NonExistentLead_ReturnsFail()
    {
        // Return null when looking up lead ID 99 (does not exist)
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var handler = CreateHandler();

        // Try to delete a lead that does not exist
        var result = await handler.HandleAsync(new DeleteLeadCommand(99));

        // Should fail with a "not found" error
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    // TEST: Deleting a converted lead should fail because converted leads must be preserved
    [Fact]
    public async Task DeleteLead_ConvertedLead_ReturnsFail()
    {
        // Set up a lead with "Converted" status — it is protected from deletion
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "Converted" });
        var handler = CreateHandler();

        // Try to delete the converted lead
        var result = await handler.HandleAsync(new DeleteLeadCommand(1));

        // Should fail because converted leads cannot be deleted
        Assert.False(result.Success);
        Assert.Contains("Cannot delete a converted lead", result.Message);
    }
}

/*
 * FILE SUMMARY:
 * This file tests the DeleteLeadHandler, which handles removing leads from the system.
 * It verifies that a normal lead can be deleted successfully, that trying to delete a
 * non-existent lead returns a "not found" error, and that converted leads are protected
 * from deletion. Converted leads are treated as important records that must be preserved.
 * Moq fakes simulate the database so tests run without any external dependencies.
 */
