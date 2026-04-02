// Import the interaction feature handlers and ASP.NET MVC classes
using LeadManagementSystem.Features.Interactions;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementInteractionsService.Controllers;

// This controller handles all HTTP requests related to interactions (calls, emails, meetings, etc.)
// [ApiController] enables automatic model validation and binding
// [Route] sets the base URL path to "api/interactions"
[ApiController]
[Route("api/[controller]")]
public sealed class InteractionsController : ControllerBase
{
    // These handlers process the actual business logic (CQRS pattern — one handler per operation)
    private readonly GetInteractionsByLeadHandler _getByLeadHandler;
    private readonly CreateInteractionHandler _createHandler;

    // Constructor — ASP.NET automatically injects the handlers (dependency injection)
    public InteractionsController(
        GetInteractionsByLeadHandler getByLeadHandler,
        CreateInteractionHandler createHandler)
    {
        _getByLeadHandler = getByLeadHandler;
        _createHandler = createHandler;
    }

    // GET api/interactions/lead/{leadId} — Fetch all interactions for a specific lead
    [HttpGet("lead/{leadId:int}")]
    public async Task<ActionResult> GetByLead(int leadId)
    {
        // Ask the query handler to get all interactions belonging to this lead
        var items = await _getByLeadHandler.HandleAsync(new GetInteractionsByLeadQuery(leadId));
        // Return the list with a 200 OK status
        return Ok(items);
    }

    // POST api/interactions — Create a new interaction record
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateInteractionRequest request)
    {
        // Build a command from the request data and send it to the handler
        var result = await _createHandler.HandleAsync(new CreateInteractionCommand(
            request.InteractionType,
            request.Details,
            request.InteractionDate,
            request.FollowUpDate,
            request.LeadId));

        // If successful, return 201 Created with the new interaction's ID
        // If something went wrong, return 400 Bad Request with an error message
        return result.Success
            ? Created($"/api/interactions/{result.Value}", new { id = result.Value, message = result.Message })
            : BadRequest(new { message = result.Message });
    }
}

// Data Transfer Object (DTO) — defines the shape of data the client sends when creating an interaction
public sealed record CreateInteractionRequest(
    string InteractionType,
    string Details,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);

/*
    FILE SUMMARY:
    This controller is the HTTP entry point for all interaction-related API calls.
    It supports fetching interactions for a specific lead and creating new interactions.
    It uses CQRS handlers to separate read (query) and write (command) operations.
    The CreateInteractionRequest record defines the expected JSON body for creating interactions.
    All responses follow standard REST conventions (200 OK, 201 Created, 400 Bad Request).
*/
