using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadManagementTests;

public class DeleteLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<DeleteLeadHandler>> _logger = new();

    private DeleteLeadHandler CreateHandler() => new(_leadRepo.Object, _logger.Object);

    [Fact]
    public async Task DeleteLead_ExistingLead_ReturnsSuccess()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "New" });
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new DeleteLeadCommand(1));

        Assert.True(result.Success);
        _leadRepo.Verify(r => r.DeleteLead(1), Times.Once);
    }

    [Fact]
    public async Task DeleteLead_NonExistentLead_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new DeleteLeadCommand(99));

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task DeleteLead_ConvertedLead_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(new Lead { LeadId = 1, Name = "Test", Status = "Converted" });
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new DeleteLeadCommand(1));

        Assert.False(result.Success);
        Assert.Contains("Cannot delete a converted lead", result.Message);
    }
}
