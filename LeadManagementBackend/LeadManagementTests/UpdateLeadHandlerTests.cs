using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadManagementTests;

public class UpdateLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ISalesRepository> _salesRepo = new();
    private readonly Mock<ILogger<UpdateLeadHandler>> _logger = new();

    private UpdateLeadHandler CreateHandler() => new(_leadRepo.Object, _salesRepo.Object, _logger.Object);

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

    [Fact]
    public async Task UpdateLead_ValidInput_ReturnsSuccess()
    {
        var existing = CreateExistingLead();
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(existing);
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead> { existing });
        var handler = CreateHandler();
        var cmd = new UpdateLeadCommand(1, "Updated Name", "existing@test.com", null, null, null, "New", "Website", "High", null);

        var result = await handler.HandleAsync(cmd);

        Assert.True(result.Success);
        _leadRepo.Verify(r => r.UpdateLead(It.IsAny<Lead>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLead_NonExistentLead_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(99)).Returns((Lead?)null);
        var handler = CreateHandler();
        var cmd = new UpdateLeadCommand(99, "Name", null, null, null, null, "New", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task UpdateLead_ConvertedLead_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(CreateExistingLead("Converted"));
        var handler = CreateHandler();
        var cmd = new UpdateLeadCommand(1, "Name", null, null, null, null, "Converted", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("cannot be modified", result.Message, StringComparison.OrdinalIgnoreCase);
    }

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

        Assert.False(result.Success);
        Assert.Contains("Cannot transition", result.Message);
    }

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

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateLead_DuplicateEmailDifferentLead_ReturnsFail()
    {
        var lead1 = CreateExistingLead();
        var lead2 = new Lead { LeadId = 2, Email = "taken@test.com", Name = "Other", Status = "New" };
        _leadRepo.Setup(r => r.GetLeadById(1)).Returns(lead1);
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead> { lead1, lead2 });
        var handler = CreateHandler();
        var cmd = new UpdateLeadCommand(1, "Name", "taken@test.com", null, null, null, "New", "Website", "Medium", null);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("email already exists", result.Message);
    }
}
