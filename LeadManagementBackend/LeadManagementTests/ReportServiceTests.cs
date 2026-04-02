// Import repository interfaces (contracts for data access)
using LeadManagementSystem.Interfaces;
// Import the ReportService which calculates analytics and statistics
using LeadManagementSystem.Logic;
// Import the Lead model
using LeadManagementSystem.Models;
// Import Moq for creating fake dependencies
using Moq;

namespace LeadManagementTests;

// This class tests the ReportService, which generates analytics reports about leads
public class ReportServiceTests
{
    // Create a fake version of the lead repository
    private readonly Mock<ILeadRepository> _leadRepo = new();

    // TEST: Status distribution should correctly count how many leads are in each status
    [Fact]
    public void GetLeadStatusDistribution_ReturnsCorrectGroups()
    {
        // Set up 3 leads: 2 are "New" and 1 is "Qualified"
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", Status = "New" },
            new() { LeadId = 2, Name = "B", Status = "New" },
            new() { LeadId = 3, Name = "C", Status = "Qualified" },
        });
        var service = new ReportService(_leadRepo.Object);

        // Get the status distribution report
        var result = service.GetLeadStatusDistribution();

        // Should return 2 groups: "New" with count 2, and "Qualified" with count 1
        Assert.Equal(2, result.Count);
        Assert.Equal("New", result[0].Status);
        Assert.Equal(2, result[0].Count);
        Assert.Equal("Qualified", result[1].Status);
        Assert.Equal(1, result[1].Count);
    }

    // TEST: Conversion rate should correctly calculate the percentage of converted leads
    [Fact]
    public void GetConversionRate_ReturnsCorrectRate()
    {
        // Set up 4 leads: 2 converted, 2 new → 50% conversion rate
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", Status = "Converted" },
            new() { LeadId = 2, Name = "B", Status = "New" },
            new() { LeadId = 3, Name = "C", Status = "New" },
            new() { LeadId = 4, Name = "D", Status = "Converted" },
        });
        var service = new ReportService(_leadRepo.Object);

        // Get the conversion rate report
        var result = service.GetConversionRate();

        // Should show 4 total leads, 2 converted, and a 50% conversion rate
        Assert.Equal(4, result.TotalLeads);
        Assert.Equal(2, result.ConvertedLeads);
        Assert.Equal(50.0, result.ConversionRate);
    }

    // TEST: Conversion rate with no leads should return all zeros (no division by zero error)
    [Fact]
    public void GetConversionRate_EmptyLeads_ReturnsZero()
    {
        // Set up an empty list of leads
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>());
        var service = new ReportService(_leadRepo.Object);

        var result = service.GetConversionRate();

        // All values should be zero when there are no leads
        Assert.Equal(0, result.TotalLeads);
        Assert.Equal(0, result.ConvertedLeads);
        Assert.Equal(0, result.ConversionRate);
    }

    // TEST: Leads grouped by source should show the correct count per source
    [Fact]
    public void GetLeadsBySource_GroupsCorrectly()
    {
        // Set up 3 leads: 2 from "Website" and 1 from "Referral"
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", Source = "Website" },
            new() { LeadId = 2, Name = "B", Source = "Website" },
            new() { LeadId = 3, Name = "C", Source = "Referral" },
        });
        var service = new ReportService(_leadRepo.Object);

        // Get the leads-by-source report
        var result = service.GetLeadsBySource();

        // Should return 2 groups with correct counts
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Source == "Website" && s.Count == 2);
        Assert.Contains(result, s => s.Source == "Referral" && s.Count == 1);
    }

    // TEST: Leads grouped by sales rep should count assigned and converted leads per rep
    [Fact]
    public void GetLeadsBySalesRep_GroupsCorrectly()
    {
        // Set up 4 leads: rep 10 has 2 leads (1 converted), rep 20 has 1 lead, 1 is unassigned
        _leadRepo.Setup(r => r.GetAllLeads()).Returns(new List<Lead>
        {
            new() { LeadId = 1, Name = "A", AssignedToRepId = 10, Status = "New" },
            new() { LeadId = 2, Name = "B", AssignedToRepId = 10, Status = "Converted" },
            new() { LeadId = 3, Name = "C", AssignedToRepId = 20, Status = "New" },
            new() { LeadId = 4, Name = "D" }, // unassigned lead (no rep)
        });
        var service = new ReportService(_leadRepo.Object);

        // Get the leads-by-sales-rep report
        var result = service.GetLeadsBySalesRep();

        // Should return 2 reps (unassigned leads are excluded from this report)
        Assert.Equal(2, result.Count);
        // Rep 10 has 2 assigned leads, 1 of which is converted
        var rep10 = result.First(r => r.SalesRepId == 10);
        Assert.Equal(2, rep10.AssignedCount);
        Assert.Equal(1, rep10.ConvertedCount);
        // Rep 20 has 1 assigned lead, none converted
        var rep20 = result.First(r => r.SalesRepId == 20);
        Assert.Equal(1, rep20.AssignedCount);
        Assert.Equal(0, rep20.ConvertedCount);
    }
}

/*
 * FILE SUMMARY:
 * This file tests the ReportService, which generates analytics dashboards for the lead management system.
 * It tests four reports: lead status distribution, conversion rate, leads grouped by source, and leads
 * grouped by sales rep. Each test verifies that the grouping and counting math is correct.
 * It also checks the edge case of zero leads to ensure there are no division-by-zero errors.
 * These reports are used by managers and admins to track team performance and lead pipeline health.
 */
