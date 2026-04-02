using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.SalesReps;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementSalesRepsService.Controllers;

[ApiController]
[Route("api/reps")]
public sealed class SalesRepsController : ControllerBase
{
    private readonly GetAllSalesRepsHandler _getAllHandler;
    private readonly GetSalesRepByIdHandler _getByIdHandler;
    private readonly CreateSalesRepHandler _createHandler;
    private readonly UpdateSalesRepHandler _updateHandler;
    private readonly DeleteSalesRepHandler _deleteHandler;

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

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var reps = await _getAllHandler.HandleAsync(new GetAllSalesRepsQuery());
        return Ok(reps);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var rep = await _getByIdHandler.HandleAsync(new GetSalesRepByIdQuery(id));
        return rep is null ? NotFound(new { message = "Sales representative not found." }) : Ok(rep);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSalesRepRequest request)
    {
        var result = await _createHandler.HandleAsync(new CreateSalesRepCommand(request.Name, request.Email, request.Department));
        return result.Success
            ? Created($"/api/reps/{result.Value}", new { id = result.Value, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSalesRepRequest request)
    {
        var result = await _updateHandler.HandleAsync(new UpdateSalesRepCommand(id, request.Name, request.Email, request.Department));
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteSalesRepCommand(id));
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

public sealed record CreateSalesRepRequest(string Name, string Email, string? Department);

public sealed record UpdateSalesRepRequest(string Name, string Email, string Department);
