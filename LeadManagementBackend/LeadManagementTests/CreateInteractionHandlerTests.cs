using LeadManagementSystem.Features.Interactions;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadManagementTests;

public class CreateInteractionHandlerTests
{
    private readonly Mock<IInteractionRepository> _interactionRepo = new();
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<CreateInteractionHandler>> _logger = new();

    private CreateInteractionHandler CreateHandler() => new(_interactionRepo.Object, _leadRepo.Object, _logger.Object);

    [Fact]
    public async Task CreateInteraction_ValidInput_ReturnsSuccess()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();
        var cmd = new CreateInteractionCommand("Call", "Discussed pricing", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddDays(7), 1);

        var result = await handler.HandleAsync(cmd);

        Assert.True(result.Success);
        _interactionRepo.Verify(r => r.AddInteraction(It.IsAny<Interaction>()), Times.Once);
    }

    [Fact]
    public async Task CreateInteraction_ConvertedLead_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "Converted" });
        var handler = CreateHandler();
        var cmd = new CreateInteractionCommand("Call", "Notes", DateTime.UtcNow.AddHours(-1), null, 1);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("Cannot add interactions to a converted lead", result.Message);
    }

    [Fact]
    public async Task CreateInteraction_FutureDate_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();
        var cmd = new CreateInteractionCommand("Call", "Notes", DateTime.UtcNow.AddDays(5), null, 1);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("cannot be in the future", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateInteraction_FollowUpBeforeInteraction_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();
        var interactionDate = DateTime.UtcNow.AddHours(-2);
        var cmd = new CreateInteractionCommand("Call", "Notes", interactionDate, interactionDate.AddHours(-1), 1);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("FollowUpDate must be greater than InteractionDate", result.Message);
    }

    [Fact]
    public async Task CreateInteraction_NonExistentLead_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var handler = CreateHandler();
        var cmd = new CreateInteractionCommand("Call", "Notes", DateTime.UtcNow.AddHours(-1), null, 99);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("Lead not found", result.Message);
    }
}
