// This "using" line imports the report service that calculates statistics
using LeadManagementSystem.Logic;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Reports;

// This is a Query = a question that reads data (it doesn't change anything)
// It takes no parameters because it calculates a summary for ALL leads
public sealed record GetLeadStatusDistributionQuery();

// This is the handler — it contains the logic to answer the query above
public sealed class GetLeadStatusDistributionHandler
{
    // _reportService contains the logic for generating reports and statistics
    private readonly ReportService _reportService;

    // The constructor receives the report service tool when this handler is created
    public GetLeadStatusDistributionHandler(ReportService reportService)
    {
        _reportService = reportService;
    }

    // This method runs when someone asks "how many leads are in each status?"
    // It returns a list showing counts like: New=10, Contacted=5, Qualified=3, etc.
    public Task<List<LeadStatusStat>> HandleAsync(GetLeadStatusDistributionQuery request)
    {
        return Task.FromResult(_reportService.GetLeadStatusDistribution());
    }
}

/*
 * FILE SUMMARY: GetLeadStatusDistributionQuery.cs
 *
 * This file handles generating a report that shows how many leads are in each status
 * (Query = question that reads data). For example, it might return: "New: 10, Contacted: 5,
 * Qualified: 3, Converted: 2, Unqualified: 1". This gives managers a bird's-eye view of the sales pipeline.
 * The actual counting logic lives in the ReportService — this handler just calls it and returns the results.
 * This is used to power dashboard charts and analytics in the frontend.
 */
