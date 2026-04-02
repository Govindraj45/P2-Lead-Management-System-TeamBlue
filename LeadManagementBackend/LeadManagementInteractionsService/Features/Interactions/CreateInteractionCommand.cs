// Import repository interfaces, models, and the shared OperationResult helper
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;

namespace LeadManagementSystem.Features.Interactions;

// Command record — holds all the data needed to create a new interaction
// Records are immutable (cannot be changed after creation), which keeps data safe
public sealed record CreateInteractionCommand(
    string InteractionType,
    string Details,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);

// Handler — contains the business logic for creating an interaction
public sealed class CreateInteractionHandler
{
    // Repository for saving interactions to the database
    private readonly IInteractionRepository _repository;
    // Repository for looking up leads (we need to verify the lead exists)
    private readonly ILeadRepository _leadRepository;

    // Constructor — receives repositories via dependency injection
    public CreateInteractionHandler(IInteractionRepository repository, ILeadRepository leadRepository)
    {
        _repository = repository;
        _leadRepository = leadRepository;
    }

    // Main method — processes the command and returns success or failure
    public Task<OperationResult<int>> HandleAsync(CreateInteractionCommand request)
    {
        // First, check if the lead exists in the database
        var lead = _leadRepository.GetLeadById(request.LeadId);
        if (lead is null)
        {
            // If the lead doesn't exist, return a failure result
            return Task.FromResult(OperationResult<int>.Fail("Lead not found."));
        }

        // Build a new Interaction object from the command data
        var interaction = new Interaction
        {
            InteractionType = request.InteractionType,
            Notes = request.Details,
            // Use the provided date, or default to "right now" in UTC
            InteractionDate = request.InteractionDate ?? DateTime.UtcNow,
            FollowUpDate = request.FollowUpDate,
            LeadId = request.LeadId
        };

        // Save the interaction to the database
        _repository.AddInteraction(interaction);
        // Return success with the new interaction's ID
        return Task.FromResult(OperationResult<int>.Ok(interaction.InteractionId, "Interaction created successfully."));
    }
}

/*
    FILE SUMMARY:
    This file implements the "Create Interaction" command in the CQRS pattern.
    The CreateInteractionCommand record carries the data needed to log a new interaction.
    The CreateInteractionHandler validates that the lead exists, builds an Interaction
    object, saves it to the database, and returns an OperationResult indicating success or failure.
    This keeps write logic cleanly separated from read (query) logic.
*/
