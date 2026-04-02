using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
using LeadManagementSystem.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadManagementTests;

public class UpdateLeadStatusHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<UpdateLeadStatusHandler>> _logger = new();

    [Fact]
    public async Task UpdateStatus_ValidTransition_ReturnsSuccess()
    {
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new UpdateLeadStatusHandler(service, _logger.Object);

        var result = await handler.HandleAsync(new UpdateLeadStatusCommand(1, "Contacted"));

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateStatus_InvalidTransition_ReturnsFail()
    {
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new UpdateLeadStatusHandler(service, _logger.Object);

        var result = await handler.HandleAsync(new UpdateLeadStatusCommand(1, "Converted"));

        Assert.False(result.Success);
        Assert.Contains("Cannot transition", result.Message);
    }

    [Fact]
    public async Task UpdateStatus_ConvertedLead_ReturnsFail()
    {
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "Converted" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new UpdateLeadStatusHandler(service, _logger.Object);

        var result = await handler.HandleAsync(new UpdateLeadStatusCommand(1, "New"));

        Assert.False(result.Success);
    }
}
