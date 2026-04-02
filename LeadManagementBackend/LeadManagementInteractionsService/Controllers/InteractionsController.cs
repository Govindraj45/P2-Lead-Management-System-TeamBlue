using LeadManagementSystem.Features.Interactions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagementInteractionsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InteractionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InteractionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("lead/{leadId:int}")]
    public async Task<ActionResult> GetByLead(int leadId)
    {
        var items = await _mediator.Send(new GetInteractionsByLeadQuery(leadId));
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateInteractionRequest request)
    {
        var result = await _mediator.Send(new CreateInteractionCommand(
            request.InteractionType,
            request.Details,
            request.InteractionDate,
            request.FollowUpDate,
            request.LeadId));

        return result.Success
            ? Created($"/api/interactions/{result.Value}", new { id = result.Value, message = result.Message })
            : BadRequest(new { message = result.Message });
    }
}

public sealed record CreateInteractionRequest(
    string InteractionType,
    string Details,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);
