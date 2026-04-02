using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
using LeadManagementSystem.Models;
using Moq;

namespace LeadManagementTests;

public class ReportServiceTests
{
    private readonly Mock<ILeadRepository> _leadRepo = new();

    [Fact]
    public void GetLeadStatusDistribution_ReturnsCorrectGroups()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", Status = "New" },
            new() { LeadId = 2, Name = "B", Status = "New" },
            new() { LeadId = 3, Name = "C", Status = "Qualified" },
        });
        var service = new ReportService(_leadRepo.Object);

        var result = service.GetLeadStatusDistribution();

        Assert.Equal(2, result.Count);
        Assert.Equal("New", result[0].Status);
        Assert.Equal(2, result[0].Count);
        Assert.Equal("Qualified", result[1].Status);
        Assert.Equal(1, result[1].Count);
    }

    [Fact]
    public void GetConversionRate_ReturnsCorrectRate()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", Status = "Converted" },
            new() { LeadId = 2, Name = "B", Status = "New" },
            new() { LeadId = 3, Name = "C", Status = "New" },
            new() { LeadId = 4, Name = "D", Status = "Converted" },
        });
        var service = new ReportService(_leadRepo.Object);

        var result = service.GetConversionRate();

        Assert.Equal(4, result.TotalLeads);
        Assert.Equal(2, result.ConvertedLeads);
        Assert.Equal(50.0, result.ConversionRate);
    }

    [Fact]
    public void GetConversionRate_EmptyLeads_ReturnsZero()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var service = new ReportService(_leadRepo.Object);

        var result = service.GetConversionRate();

        Assert.Equal(0, result.TotalLeads);
        Assert.Equal(0, result.ConvertedLeads);
        Assert.Equal(0, result.ConversionRate);
    }

    [Fact]
    public void GetLeadsBySource_GroupsCorrectly()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", Source = "Website" },
            new() { LeadId = 2, Name = "B", Source = "Website" },
            new() { LeadId = 3, Name = "C", Source = "Referral" },
        });
        var service = new ReportService(_leadRepo.Object);

        var result = service.GetLeadsBySource();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Source == "Website" && s.Count == 2);
        Assert.Contains(result, s => s.Source == "Referral" && s.Count == 1);
    }

    [Fact]
    public void GetLeadsBySalesRep_GroupsCorrectly()
    {
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", AssignedToRepId = 10, Status = "New" },
            new() { LeadId = 2, Name = "B", AssignedToRepId = 10, Status = "Converted" },
            new() { LeadId = 3, Name = "C", AssignedToRepId = 20, Status = "New" },
            new() { LeadId = 4, Name = "D" }, // unassigned
        });
        var service = new ReportService(_leadRepo.Object);

        var result = service.GetLeadsBySalesRep();

        Assert.Equal(2, result.Count);
        var rep10 = result.First(r => r.SalesRepId == 10);
        Assert.Equal(2, rep10.AssignedCount);
        Assert.Equal(1, rep10.ConvertedCount);
        var rep20 = result.First(r => r.SalesRepId == 20);
        Assert.Equal(1, rep20.AssignedCount);
        Assert.Equal(0, rep20.ConvertedCount);
    }
}
