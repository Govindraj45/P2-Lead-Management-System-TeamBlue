// Import the ReportService which contains the business logic for generating reports
using LeadManagementSystem.Logic;

namespace LeadManagementSystem.Features.Reports;

// Query record — this query takes no parameters because it returns data for ALL leads
// In CQRS, queries are read-only operations that never change data
public sealed record GetLeadStatusDistributionQuery();

// Handler — contains the logic for fetching the lead status distribution report
public sealed class GetLeadStatusDistributionHandler
{
    // The ReportService does the heavy lifting of querying and grouping lead data
    private readonly ReportService _reportService;

    // Constructor — receives the report service via dependency injection
    public GetLeadStatusDistributionHandler(ReportService reportService)
    {
        _reportService = reportService;
    }

    // Main method — asks the ReportService to compute the status distribution
    // Returns a list of LeadStatusStat objects (each has a status name and a count)
    public Task<List<LeadStatusStat>> HandleAsync(GetLeadStatusDistributionQuery request)
    {
        return Task.FromResult(_reportService.GetLeadStatusDistribution());
    }
}

/*
    FILE SUMMARY:
    This file implements the "Get Lead Status Distribution" query in the CQRS pattern.
    The query takes no parameters since it aggregates data across all leads.
    The handler delegates to ReportService, which groups leads by status and counts them.
    The result is a list showing how many leads are in each status (e.g., New, Contacted, Qualified).
*/
