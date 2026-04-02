// Import shared types for operation results and lead feature handlers
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Leads;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementLeadsService.Controllers;

// This controller handles all HTTP requests related to leads (CRUD operations)
// It listens on the route "/api/leads"
[ApiController]
[Route("api/[controller]")]
public sealed class LeadsController : ControllerBase
{
    // Each handler is responsible for one specific operation (CQRS pattern)
    private readonly GetAllLeadsHandler _getAllHandler;
    private readonly GetLeadByIdHandler _getByIdHandler;
    private readonly CreateLeadHandler _createHandler;
    private readonly UpdateLeadHandler _updateHandler;
    private readonly DeleteLeadHandler _deleteHandler;
    private readonly UpdateLeadStatusHandler _statusHandler;
    private readonly ConvertLeadToCustomerHandler _convertHandler;

    // Constructor: .NET injects all the handlers automatically (dependency injection)
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

    // GET /api/leads — Returns a list of all leads
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var leads = await _getAllHandler.HandleAsync(new GetAllLeadsQuery());
        return Ok(leads);
    }

    // GET /api/leads/{id} — Returns a single lead by its ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var lead = await _getByIdHandler.HandleAsync(new GetLeadByIdQuery(id));
        // Return 404 if the lead doesn't exist, otherwise return the lead
        return lead is null ? NotFound(new { message = "Lead not found." }) : Ok(lead);
    }

    // POST /api/leads — Creates a new lead from the request body
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateLeadRequest request)
    {
        // Build a command from the request and pass it to the handler
        var result = await _createHandler.HandleAsync(new CreateLeadCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.Company,
            request.Status,
            request.Source,
            request.Priority,
            request.AssignedToRepId));

        // Return 201 Created on success, or 400 Bad Request on failure
        return result.Success
            ? Created($"/api/leads/{result.Value}", new { id = result.Value, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    // PUT /api/leads/{id} — Updates all fields of an existing lead
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

    // DELETE /api/leads/{id} — Deletes a lead by its ID
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteLeadCommand(id));
        return ToActionResult(result);
    }

    // PUT /api/leads/{id}/status — Updates only the status of a lead (e.g., New → Contacted)
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] LeadStatusUpdateRequest request)
    {
        var result = await _statusHandler.HandleAsync(new UpdateLeadStatusCommand(id, request.NewStatus));
        return ToActionResult(result);
    }

    // POST /api/leads/{id}/convert — Converts a qualified lead into a customer
    [HttpPost("{id:int}/convert")]
    public async Task<ActionResult> ConvertToCustomer(int id)
    {
        var result = await _convertHandler.HandleAsync(new ConvertLeadToCustomerCommand(id));
        return ToActionResult(result);
    }

    // Helper method: converts an OperationResult into the correct HTTP response
    private ActionResult ToActionResult(OperationResult result)
    {
        if (result.Success)
        {
            return Ok(new { message = result.Message });
        }

        // Return 404 if the message says "not found", otherwise return 400
        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(new { message = result.Message })
            : BadRequest(new { message = result.Message });
    }
}

// Request DTO for creating a new lead (sent in the POST body)
public sealed record CreateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedToRepId);

// Request DTO for updating an existing lead (sent in the PUT body)
public sealed record UpdateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string Status,
    string Source,
    string Priority,
    int? AssignedToRepId);

// Request DTO for changing a lead's status (sent in the PUT body)
public sealed record LeadStatusUpdateRequest(string NewStatus);

/*
 * FILE SUMMARY:
 * This is the API controller for leads — it handles all HTTP requests at /api/leads.
 * It supports full CRUD: creating, reading, updating, and deleting leads, plus status updates and lead conversion.
 * Each action delegates work to a dedicated CQRS handler, keeping the controller thin and focused on HTTP concerns.
 * Request DTOs (CreateLeadRequest, UpdateLeadRequest, etc.) define the shape of incoming JSON data.
 * This follows the CQRS pattern without MediatR, using simple handler classes instead.
 */
