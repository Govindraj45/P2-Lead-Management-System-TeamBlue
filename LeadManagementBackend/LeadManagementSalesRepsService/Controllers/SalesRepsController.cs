using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.SalesReps;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementSalesRepsService.Controllers;

[ApiController]
[Route("api/reps")]
public sealed class SalesRepsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesRepsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var reps = await _mediator.Send(new GetAllSalesRepsQuery());
        return Ok(reps);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var rep = await _mediator.Send(new GetSalesRepByIdQuery(id));
        return rep is null ? NotFound(new { message = "Sales representative not found." }) : Ok(rep);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSalesRepRequest request)
    {
        var result = await _mediator.Send(new CreateSalesRepCommand(request.Name, request.Email, request.Department));
        return result.Success
            ? Created($"/api/reps/{result.Value}", new { id = result.Value, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSalesRepRequest request)
    {
        var result = await _mediator.Send(new UpdateSalesRepCommand(id, request.Name, request.Email, request.Department));
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteSalesRepCommand(id));
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
