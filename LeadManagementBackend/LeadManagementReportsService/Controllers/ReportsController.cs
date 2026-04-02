// Import the report feature handler and ASP.NET MVC classes
using LeadManagementSystem.Features.Reports;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementReportsService.Controllers;

// This controller handles all HTTP requests related to reports and analytics
// [ApiController] enables automatic model validation and binding
// [Route] sets the base URL path to "api/reports"
[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    // Handler that processes the lead status distribution query
    private readonly GetLeadStatusDistributionHandler _statusDistributionHandler;

    // Constructor — ASP.NET automatically injects the handler (dependency injection)
    public ReportsController(GetLeadStatusDistributionHandler statusDistributionHandler)
    {
        _statusDistributionHandler = statusDistributionHandler;
    }

    // GET api/reports/status-distribution — Returns a breakdown of leads grouped by their status
    [HttpGet("status-distribution")]
    public async Task<ActionResult> GetStatusDistribution()
    {
        // Ask the handler to compute the status distribution (e.g., how many leads are New, Contacted, etc.)
        var distribution = await _statusDistributionHandler.HandleAsync(new GetLeadStatusDistributionQuery());
        // Return the distribution data with a 200 OK status
        return Ok(distribution);
    }
}

/*
    FILE SUMMARY:
    This controller is the HTTP entry point for report-related API calls.
    It currently supports one endpoint that returns how leads are distributed across statuses.
    It uses the CQRS query pattern, delegating the work to GetLeadStatusDistributionHandler.
    This is a read-only controller — it only fetches analytics data, never modifies anything.
*/
