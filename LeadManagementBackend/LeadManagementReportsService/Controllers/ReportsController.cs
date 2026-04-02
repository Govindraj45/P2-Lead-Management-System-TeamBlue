using LeadManagementSystem.Features.Reports;
using LeadManagementSystem.Logic;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LeadManagementReportsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ReportService _reportService;
    private readonly IDistributedCache _cache;

    public ReportsController(IMediator mediator, ReportService reportService, IDistributedCache cache)
    {
        _mediator = mediator;
        _reportService = reportService;
        _cache = cache;
    }

    [HttpGet("status-distribution")]
    public async Task<ActionResult> GetStatusDistribution()
    {
        var data = await GetCachedOrCompute("analytics:by-status", () => _reportService.GetLeadStatusDistribution());
        return Ok(data);
    }

    [HttpGet("by-source")]
    public async Task<ActionResult> GetBySource()
    {
        var data = await GetCachedOrCompute("analytics:by-source", () => _reportService.GetLeadsBySource());
        return Ok(data);
    }

    [HttpGet("conversion-rate")]
    public async Task<ActionResult> GetConversionRate()
    {
        var data = await GetCachedOrCompute("analytics:conversion-rate", () => _reportService.GetConversionRate());
        return Ok(data);
    }

    [HttpGet("by-salesrep")]
    public async Task<ActionResult> GetBySalesRep()
    {
        var data = await GetCachedOrCompute("analytics:by-salesrep", () => _reportService.GetLeadsBySalesRep());
        return Ok(data);
    }

    private async Task<T> GetCachedOrCompute<T>(string key, Func<T> compute)
    {
        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<T>(cached)!;

        var data = compute();
        var json = JsonSerializer.Serialize(data);
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        return data;
    }
}
