using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadManagementTests;

public class CreateLeadHandlerTests
{
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<ISalesRepository> _salesRepo = new();
    private readonly Mock<ILogger<CreateLeadHandler>> _logger = new();

    private CreateLeadHandler CreateHandler() => new(_leadRepo.Object, _salesRepo.Object, _logger.Object);

    [Fact]
    public async Task CreateLead_ValidInput_ReturnsSuccess()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var handler = CreateHandler();
        var cmd = new CreateLeadCommand("John Doe", "john@test.com", "1234567890", "Acme", "Dev", "New", "Website", "High", null);

        var result = await handler.HandleAsync(cmd);

        Assert.True(result.Success);
        _leadRepo.Verify(r => r.AddLead(It.IsAny<Lead>()), Times.Once);
    }

    [Fact]
    public async Task CreateLead_EmptyName_ReturnsFail()
    {
        var handler = CreateHandler();
        var cmd = new CreateLeadCommand("", "john@test.com", null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("Name is required", result.Message);
    }

    [Fact]
    public async Task CreateLead_InvalidEmail_ReturnsFail()
    {
        var handler = CreateHandler();
        var cmd = new CreateLeadCommand("John", "not-an-email", null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("Email must be a valid format", result.Message);
    }

    [Fact]
    public async Task CreateLead_DuplicateEmail_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new Lead { LeadId = 1, Email = "john@test.com", Name = "Existing" }
        });
        var handler = CreateHandler();
        var cmd = new CreateLeadCommand("John", "john@test.com", null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("email already exists", result.Message);
    }

    [Fact]
    public async Task CreateLead_InvalidPhone_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var handler = CreateHandler();
        var cmd = new CreateLeadCommand("John", null, "abc", null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("Phone must follow a valid format", result.Message);
    }

    [Fact]
    public async Task CreateLead_NonExistentSalesRep_ReturnsFail()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        _salesRepo.Setup(r => r.GetRepById(It.IsAny<int>())).Returns((SalesRep?)null);
        var handler = CreateHandler();
        var cmd = new CreateLeadCommand("John", null, null, null, null, null, null, null, 999);

        var result = await handler.HandleAsync(cmd);

        Assert.False(result.Success);
        Assert.Contains("does not reference an existing sales rep", result.Message);
    }

    [Fact]
    public async Task CreateLead_NullOptionalFields_ReturnsSuccess()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var handler = CreateHandler();
        var cmd = new CreateLeadCommand("Jane", null, null, null, null, null, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.True(result.Success);
        Assert.Contains("created successfully", result.Message);
    }
}
