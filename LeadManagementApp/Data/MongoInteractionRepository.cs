using LeadManagementSystem.Models;
using LeadManagementSystem.Interfaces;
using MongoDB.Driver;

namespace LeadManagementSystem.Data;

public class MongoInteractionRepository : IInteractionRepository
{
    private readonly MongoDbContext _context;
    private readonly MongoSequenceService _sequenceService;

    public MongoInteractionRepository(MongoDbContext context, MongoSequenceService sequenceService)
    {
        _context = context;
        _sequenceService = sequenceService;
    }

    public void AddInteraction(Interaction interaction)
    {
        if (interaction.InteractionId <= 0)
        {
            interaction.InteractionId = _sequenceService.GetNextValue("interaction_id");
        }

        _context.Interactions.InsertOne(interaction);
    }

    public List<Interaction> GetInteractionsByLead(int leadId)
    {
        return _context.Interactions
            .Find(x => x.LeadId == leadId)
            .SortByDescending(x => x.InteractionDate)
            .ToList();
    }
}
