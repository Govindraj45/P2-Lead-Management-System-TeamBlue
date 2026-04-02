using LeadManagementSystem.Features.Reports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementReportsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("status-distribution")]
    public async Task<ActionResult> GetStatusDistribution()
    {
        var distribution = await _mediator.Send(new GetLeadStatusDistributionQuery());
        return Ok(distribution);
    }
}
