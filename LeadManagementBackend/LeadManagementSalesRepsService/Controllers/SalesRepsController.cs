// Import the shared OperationResult helper and sales rep feature handlers
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.SalesReps;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementSalesRepsService.Controllers;

// This controller handles all HTTP requests related to sales representatives
// [ApiController] enables automatic model validation and binding
// [Route] sets the base URL path to "api/reps"
[ApiController]
[Route("api/reps")]
public sealed class SalesRepsController : ControllerBase
{
    // One handler per CQRS operation — keeps each responsibility separate
    private readonly GetAllSalesRepsHandler _getAllHandler;
    private readonly GetSalesRepByIdHandler _getByIdHandler;
    private readonly CreateSalesRepHandler _createHandler;
    private readonly UpdateSalesRepHandler _updateHandler;
    private readonly DeleteSalesRepHandler _deleteHandler;

    // Constructor — ASP.NET automatically injects all five handlers (dependency injection)
    public SalesRepsController(
        GetAllSalesRepsHandler getAllHandler,
        GetSalesRepByIdHandler getByIdHandler,
        CreateSalesRepHandler createHandler,
        UpdateSalesRepHandler updateHandler,
        DeleteSalesRepHandler deleteHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    // GET api/reps — Fetch all sales representatives
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var reps = await _getAllHandler.HandleAsync(new GetAllSalesRepsQuery());
        return Ok(reps);
    }

    // GET api/reps/{id} — Fetch a single sales rep by their ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var rep = await _getByIdHandler.HandleAsync(new GetSalesRepByIdQuery(id));
        // Return 404 Not Found if the rep doesn't exist, otherwise return the rep data
        return rep is null ? NotFound(new { message = "Sales representative not found." }) : Ok(rep);
    }

    // POST api/reps — Create a new sales representative
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSalesRepRequest request)
    {
        var result = await _createHandler.HandleAsync(new CreateSalesRepCommand(request.Name, request.Email, request.Department));
        // Return 201 Created on success, or 400 Bad Request on failure
        return result.Success
            ? Created($"/api/reps/{result.Value}", new { id = result.Value, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    // PUT api/reps/{id} — Update an existing sales representative's information
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSalesRepRequest request)
    {
        var result = await _updateHandler.HandleAsync(new UpdateSalesRepCommand(id, request.Name, request.Email, request.Department));
        // Use the helper method to convert the result into the right HTTP response
        return ToActionResult(result);
    }

    // DELETE api/reps/{id} — Delete a sales representative by their ID
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteSalesRepCommand(id));
        return ToActionResult(result);
    }

    // Helper method — converts an OperationResult into the correct HTTP status code
    // Returns 200 OK on success, 404 Not Found if the message says "not found", or 400 Bad Request otherwise
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

// DTO for creating a sales rep — Department is optional (nullable)
public sealed record CreateSalesRepRequest(string Name, string Email, string? Department);

// DTO for updating a sales rep — all fields are required
public sealed record UpdateSalesRepRequest(string Name, string Email, string Department);

/*
    FILE SUMMARY:
    This controller is the HTTP entry point for all sales-representative-related API calls.
    It supports full CRUD: list all reps, get one by ID, create, update, and delete.
    Each operation is delegated to a dedicated CQRS handler to keep responsibilities separated.
    The ToActionResult helper converts OperationResult objects into proper HTTP responses.
    Request DTOs (CreateSalesRepRequest, UpdateSalesRepRequest) define the expected JSON body shapes.
*/
