using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Leads;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementLeadsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LeadsController : ControllerBase
{
    private readonly GetAllLeadsHandler _getAllHandler;
    private readonly GetLeadByIdHandler _getByIdHandler;
    private readonly CreateLeadHandler _createHandler;
    private readonly UpdateLeadHandler _updateHandler;
    private readonly DeleteLeadHandler _deleteHandler;
    private readonly UpdateLeadStatusHandler _statusHandler;
    private readonly ConvertLeadToCustomerHandler _convertHandler;

    public LeadsController(
        GetAllLeadsHandler getAllHandler,
        GetLeadByIdHandler getByIdHandler,
        CreateLeadHandler createHandler,
        UpdateLeadHandler updateHandler,
        DeleteLeadHandler deleteHandler,
        UpdateLeadStatusHandler statusHandler,
        ConvertLeadToCustomerHandler convertHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _statusHandler = statusHandler;
        _convertHandler = convertHandler;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var leads = await _getAllHandler.HandleAsync(new GetAllLeadsQuery());
        return Ok(leads);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var lead = await _getByIdHandler.HandleAsync(new GetLeadByIdQuery(id));
        return lead is null ? NotFound(new { message = "Lead not found." }) : Ok(lead);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateLeadRequest request)
    {
        var result = await _createHandler.HandleAsync(new CreateLeadCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.Company,
            request.Status,
            request.Source,
            request.Priority,
            request.AssignedToRepId));

        return result.Success
            ? Created($"/api/leads/{result.Value}", new { id = result.Value, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateLeadRequest request)
    {
        var result = await _updateHandler.HandleAsync(new UpdateLeadCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Company,
            request.Status,
            request.Source,
            request.Priority,
            request.AssignedToRepId));

        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteLeadCommand(id));
        return ToActionResult(result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] LeadStatusUpdateRequest request)
    {
        var result = await _statusHandler.HandleAsync(new UpdateLeadStatusCommand(id, request.NewStatus));
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/convert")]
    public async Task<ActionResult> ConvertToCustomer(int id)
    {
        var result = await _convertHandler.HandleAsync(new ConvertLeadToCustomerCommand(id));
        return ToActionResult(result);
    }

    private ActionResult ToActionResult(OperationResult result)
    {
        if (result.Success)
        {
            return Ok(new { message = result.Message });
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(new { message = result.Message })
            : BadRequest(new { message = result.Message });
    }
}

public sealed record CreateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedToRepId);

public sealed record UpdateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string Status,
    string Source,
    string Priority,
    int? AssignedToRepId);

public sealed record LeadStatusUpdateRequest(string NewStatus);
