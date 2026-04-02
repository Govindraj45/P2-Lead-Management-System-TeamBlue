using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
using LeadManagementSystem.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadManagementTests;

public class ConvertLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ILogger<ConvertLeadToCustomerHandler>> _logger = new();

    [Fact]
    public async Task ConvertLead_QualifiedLead_ReturnsSuccess()
    {
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "Qualified" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new ConvertLeadToCustomerHandler(service, _logger.Object);

        var result = await handler.Handle(new ConvertLeadToCustomerCommand(1), CancellationToken.None);

        Assert.True(result.Success);
        _leadRepo.Verify(r => r.UpdateLead(It.Is<Lead>(l => l.Status == "Converted")), Times.Once);
    }

    [Fact]
    public async Task ConvertLead_NonQualifiedLead_ReturnsFail()
    {
        var lead = new Lead { LeadId = 1, Name = "Test", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead);
        var service = new LeadService(_leadRepo.Object);
        var handler = new ConvertLeadToCustomerHandler(service, _logger.Object);

        var result = await handler.Handle(new ConvertLeadToCustomerCommand(1), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ConvertLead_NonExistentLead_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var service = new LeadService(_leadRepo.Object);
        var handler = new ConvertLeadToCustomerHandler(service, _logger.Object);

        var result = await handler.Handle(new ConvertLeadToCustomerCommand(99), CancellationToken.None);

        Assert.False(result.Success);
    }
}
