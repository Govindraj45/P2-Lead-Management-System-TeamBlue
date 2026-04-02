using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Features.Interactions;

public sealed record CreateInteractionCommand(
    string InteractionType,
    string Notes,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);

public sealed class CreateInteractionHandler
{
    private readonly IInteractionRepository _repository;
    private readonly ILeadRepository _leadRepository;
    private readonly ILogger<CreateInteractionHandler> _logger;

    public CreateInteractionHandler(IInteractionRepository repository, ILeadRepository leadRepository, ILogger<CreateInteractionHandler> logger)
    {
        _repository = repository;
        _leadRepository = leadRepository;
        _logger = logger;
    }

    public Task<OperationResult<int>> HandleAsync(CreateInteractionCommand request)
    {
        var lead = _leadRepository.GetLeadById(request.LeadId);
        if (lead is null)
            return Task.FromResult(OperationResult<int>.Fail("Lead not found."));

        // Converted leads cannot have new interactions
        if (lead.Status == "Converted")
            return Task.FromResult(OperationResult<int>.Fail("Cannot add interactions to a converted lead."));

        // InteractionDate cannot be in the future
        var interactionDate = request.InteractionDate ?? DateTime.UtcNow;
        if (interactionDate > DateTime.UtcNow)
            return Task.FromResult(OperationResult<int>.Fail("InteractionDate cannot be in the future."));

        // FollowUpDate must be greater than InteractionDate
        if (request.FollowUpDate.HasValue && request.FollowUpDate.Value <= interactionDate)
            return Task.FromResult(OperationResult<int>.Fail("FollowUpDate must be greater than InteractionDate."));

        var interaction = new Interaction
        {
            InteractionType = request.InteractionType,
            Notes = request.Notes,
            InteractionDate = interactionDate,
            FollowUpDate = request.FollowUpDate,
            LeadId = request.LeadId
        };

        _repository.AddInteraction(interaction);
        _logger.LogInformation("Interaction added: InteractionId={Id}, LeadId={LeadId}, Type={Type}", interaction.InteractionId, request.LeadId, request.InteractionType);
        return Task.FromResult(OperationResult<int>.Ok(interaction.InteractionId, "Interaction created successfully."));
    }
}
