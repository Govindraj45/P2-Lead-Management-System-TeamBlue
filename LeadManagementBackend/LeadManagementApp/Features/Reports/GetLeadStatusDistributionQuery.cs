using LeadManagementSystem.Logic;

namespace LeadManagementSystem.Features.Reports;

public sealed record GetLeadStatusDistributionQuery();

public sealed class GetLeadStatusDistributionHandler
{
    private readonly ReportService _reportService;

    public GetLeadStatusDistributionHandler(ReportService reportService)
    {
        _reportService = reportService;
    }

    public Task<List<LeadStatusStat>> HandleAsync(GetLeadStatusDistributionQuery request)
    {
        return Task.FromResult(_reportService.GetLeadStatusDistribution());
    }
}
