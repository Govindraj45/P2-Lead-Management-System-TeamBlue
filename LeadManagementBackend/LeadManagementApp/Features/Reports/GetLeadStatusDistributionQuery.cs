using LeadManagementSystem.Logic;
using MediatR;

namespace LeadManagementSystem.Features.Reports;

public sealed record GetLeadStatusDistributionQuery() : IRequest<List<LeadStatusStat>>;

public sealed class GetLeadStatusDistributionHandler : IRequestHandler<GetLeadStatusDistributionQuery, List<LeadStatusStat>>
{
    private readonly ReportService _reportService;

    public GetLeadStatusDistributionHandler(ReportService reportService)
    {
        _reportService = reportService;
    }

    public Task<List<LeadStatusStat>> Handle(GetLeadStatusDistributionQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_reportService.GetLeadStatusDistribution());
    }
}
