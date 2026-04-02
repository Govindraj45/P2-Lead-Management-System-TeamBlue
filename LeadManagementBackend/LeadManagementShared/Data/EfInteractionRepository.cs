using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Data;

public class EfInteractionRepository : IInteractionRepository
{
    private readonly LeadDbContext _context;

    public EfInteractionRepository(LeadDbContext context)
    {
        _context = context;
    }

    public void AddInteraction(Interaction interaction)
    {
        _context.Interactions.Add(interaction);
        _context.SaveChanges();
    }

    public List<Interaction> GetInteractionsByLead(int leadId)
    {
        return _context.Interactions
            .Where(i => i.LeadId == leadId)
            .OrderByDescending(i => i.InteractionDate)
            .ToList();
    }
}
