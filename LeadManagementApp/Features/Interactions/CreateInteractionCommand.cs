using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;
using MediatR;

namespace LeadManagementSystem.Features.Interactions;

public sealed record CreateInteractionCommand(
    string InteractionType,
    string Details,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId) : IRequest<OperationResult<int>>;

public sealed class CreateInteractionHandler : IRequestHandler<CreateInteractionCommand, OperationResult<int>>
{
    private readonly IInteractionRepository _repository;
    private readonly ILeadRepository _leadRepository;

    public CreateInteractionHandler(IInteractionRepository repository, ILeadRepository leadRepository)
    {
        _repository = repository;
        _leadRepository = leadRepository;
    }

    public Task<OperationResult<int>> Handle(CreateInteractionCommand request, CancellationToken cancellationToken)
    {
        var lead = _leadRepository.GetLeadById(request.LeadId);
        if (lead is null)
        {
            return Task.FromResult(OperationResult<int>.Fail("Lead not found."));
        }

        var interaction = new Interaction
        {
            InteractionType = request.InteractionType,
            Details = request.Details,
            InteractionDate = request.InteractionDate ?? DateTime.UtcNow,
            FollowUpDate = request.FollowUpDate,
            LeadId = request.LeadId
        };

        _repository.AddInteraction(interaction);
        return Task.FromResult(OperationResult<int>.Ok(interaction.InteractionId, "Interaction created successfully."));
    }
}
