using LeadManagementSystem.Features.Reports;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementReportsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    private readonly GetLeadStatusDistributionHandler _statusDistributionHandler;

    public ReportsController(GetLeadStatusDistributionHandler statusDistributionHandler)
    {
        _statusDistributionHandler = statusDistributionHandler;
    }

    [HttpGet("status-distribution")]
    public async Task<ActionResult> GetStatusDistribution()
    {
        var distribution = await _statusDistributionHandler.HandleAsync(new GetLeadStatusDistributionQuery());
        return Ok(distribution);
    }
}
