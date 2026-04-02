// These "using" lines import code from other parts of the project so we can use them here
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using LeadManagementSystem.Features.Common;

// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Interactions;

// This is a Command = an action that changes data
// It holds the details of a new interaction (call, email, meeting, etc.) with a lead
public sealed record CreateInteractionCommand(
    string InteractionType,
    string Notes,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);

// This is the handler — it contains the logic to create a new interaction record
public sealed class CreateInteractionHandler
{
    // _repository talks to the database to save/read interactions
    private readonly IInteractionRepository _repository;
    // _leadRepository is used to check if the lead exists
    private readonly ILeadRepository _leadRepository;
    // _logger writes messages to a log file for tracking
    private readonly ILogger<CreateInteractionHandler> _logger;

    // The constructor receives the tools this handler needs when it's created
    public CreateInteractionHandler(IInteractionRepository repository, ILeadRepository leadRepository, ILogger<CreateInteractionHandler> logger)
    {
        _repository = repository;
        _leadRepository = leadRepository;
        _logger = logger;
    }

    // This method runs when someone wants to record a new interaction with a lead
    // It returns a result that tells you if it worked and includes the new interaction's ID
    public Task<OperationResult<int>> HandleAsync(CreateInteractionCommand request)
    {
        // First, check that the lead this interaction belongs to actually exists
        var lead = _leadRepository.GetLeadById(request.LeadId);
        if (lead is null)
            return Task.FromResult(OperationResult<int>.Fail("Lead not found."));

        // Business rule: you cannot add interactions to a lead that's already been converted to a customer
        if (lead.Status == "Converted")
            return Task.FromResult(OperationResult<int>.Fail("Cannot add interactions to a converted lead."));

        // Use the provided date, or default to right now if none was given
        var interactionDate = request.InteractionDate ?? DateTime.UtcNow;
        // Validation: the interaction date cannot be in the future (you can't log something that hasn't happened)
        if (interactionDate > DateTime.UtcNow)
            return Task.FromResult(OperationResult<int>.Fail("InteractionDate cannot be in the future."));

        // Validation: if there's a follow-up date, it must be AFTER the interaction date
        if (request.FollowUpDate.HasValue && request.FollowUpDate.Value <= interactionDate)
            return Task.FromResult(OperationResult<int>.Fail("FollowUpDate must be greater than InteractionDate."));

        // All validations passed — create the interaction object
        var interaction = new Interaction
        {
            InteractionType = request.InteractionType,
            Notes = request.Notes,
            InteractionDate = interactionDate,
            FollowUpDate = request.FollowUpDate,
            LeadId = request.LeadId
        };

        // Save the new interaction to the database
        _repository.AddInteraction(interaction);
        // Write a log entry so developers can track what was created
        _logger.LogInformation("Interaction added: InteractionId={Id}, LeadId={LeadId}, Type={Type}", interaction.InteractionId, request.LeadId, request.InteractionType);
        // Return a success result with the new interaction's ID
        return Task.FromResult(OperationResult<int>.Ok(interaction.InteractionId, "Interaction created successfully."));
    }
}

/*
 * FILE SUMMARY: CreateInteractionCommand.cs
 *
 * This file handles recording a new interaction (call, email, meeting, etc.) with a lead
 * (Command = action that changes data). It validates that the lead exists, is not already converted,
 * the interaction date is not in the future, and any follow-up date comes after the interaction date.
 * Interactions are the bread and butter of the sales process — they track every touchpoint with a lead.
 * This file is essential for building a complete history of communication with each potential customer.
 */
