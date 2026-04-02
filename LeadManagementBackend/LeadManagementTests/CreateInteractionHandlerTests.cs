// Import interaction-specific command and handler classes
using LeadManagementSystem.Features.Interactions;
// Import repository interfaces (contracts for data access)
using LeadManagementSystem.Interfaces;
// Import the Lead and Interaction models
using LeadManagementSystem.Models;
// Import logging interface
using Microsoft.Extensions.Logging;
// Import Moq for creating fake dependencies
using Moq;

namespace LeadManagementTests;

// This class tests the CreateInteractionHandler (the code that logs interactions with a lead)
// An "interaction" is a record of contact with a lead, such as a phone call or email
public class CreateInteractionHandlerTests
{
    // Create fake versions of the interaction repo, lead repo, and logger
    private readonly Mock<IInteractionRepository> _interactionRepo = new();
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<CreateInteractionHandler>> _logger = new();

    // Helper method that builds a CreateInteractionHandler with fake dependencies
    private CreateInteractionHandler CreateHandler() => new(_interactionRepo.Object, _leadRepo.Object, _logger.Object);

    // TEST: Creating an interaction with all valid data should succeed
    [Fact]
    public async Task CreateInteraction_ValidInput_ReturnsSuccess()
    {
        // Set up a lead that exists and has status "New" (not converted)
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();
        // Create a "Call" interaction that happened 1 hour ago, with a follow-up in 7 days
        var cmd = new CreateInteractionCommand("Call", "Discussed pricing", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddDays(7), 1);

        var result = await handler.HandleAsync(cmd);

        // Should succeed and AddInteraction should be called once
        Assert.True(result.Success);
        _interactionRepo.Verify(r => r.AddInteraction(It.IsAny<Interaction>()), Times.Once);
    }

    // TEST: Adding an interaction to a converted lead should fail (converted = read-only)
    [Fact]
    public async Task CreateInteraction_ConvertedLead_ReturnsFail()
    {
        // Set up a lead with status "Converted" — no more interactions allowed
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "Converted" });
        var handler = CreateHandler();
        var cmd = new CreateInteractionCommand("Call", "Notes", DateTime.UtcNow.AddHours(-1), null, 1);

        var result = await handler.HandleAsync(cmd);

        // Should fail because you cannot add interactions to a converted lead
        Assert.False(result.Success);
        Assert.Contains("Cannot add interactions to a converted lead", result.Message);
    }

    // TEST: An interaction date set in the future should fail (interactions record past events)
    [Fact]
    public async Task CreateInteraction_FutureDate_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();
        // Set the interaction date 5 days in the future — this is not allowed
        var cmd = new CreateInteractionCommand("Call", "Notes", DateTime.UtcNow.AddDays(5), null, 1);

        var result = await handler.HandleAsync(cmd);

        // Should fail because interactions cannot have future dates
        Assert.False(result.Success);
        Assert.Contains("cannot be in the future", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // TEST: A follow-up date that is BEFORE the interaction date should fail
    [Fact]
    public async Task CreateInteraction_FollowUpBeforeInteraction_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();
        // The interaction happened 2 hours ago, but the follow-up is set to 1 hour BEFORE that
        var interactionDate = DateTime.UtcNow.AddHours(-2);
        var cmd = new CreateInteractionCommand("Call", "Notes", interactionDate, interactionDate.AddHours(-1), 1);

        var result = await handler.HandleAsync(cmd);

        // Should fail because follow-up must be after the interaction date
        Assert.False(result.Success);
        Assert.Contains("FollowUpDate must be greater than InteractionDate", result.Message);
    }

    // TEST: Creating an interaction for a lead that does not exist should fail
    [Fact]
    public async Task CreateInteraction_NonExistentLead_ReturnsFail()
    {
        // Return null for lead ID 99 (does not exist)
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var handler = CreateHandler();
        var cmd = new CreateInteractionCommand("Call", "Notes", DateTime.UtcNow.AddHours(-1), null, 99);

        var result = await handler.HandleAsync(cmd);

        // Should fail with a "Lead not found" error
        Assert.False(result.Success);
        Assert.Contains("Lead not found", result.Message);
    }
}

/*
 * FILE SUMMARY:
 * This file tests the CreateInteractionHandler, which records interactions (calls, emails, meetings)
 * between sales reps and leads. It verifies that valid interactions are saved, and that invalid
 * scenarios are rejected: adding interactions to converted leads, using future dates, setting
 * follow-up dates before the interaction date, and referencing non-existent leads.
 * These rules ensure data integrity and that the interaction timeline makes logical sense.
 */
