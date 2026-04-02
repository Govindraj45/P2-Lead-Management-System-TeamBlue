// Bring in the interface this class implements, the Interaction model, and Entity Framework tools
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Data;

// This class is the REAL implementation of IInteractionRepository
// It contains the actual database code for adding and retrieving interactions
// "Ef" stands for "Entity Framework" — the tool we use to talk to SQL Server
public class EfInteractionRepository : IInteractionRepository
{
    // This is our database connection — we use it in every method below
    private readonly LeadDbContext _context;

    // The constructor receives the database context through "dependency injection"
    // This means the app automatically provides the database connection when this class is created
    public EfInteractionRepository(LeadDbContext context)
    {
        _context = context;
    }

    // Save a new interaction (call, email, or meeting) to the database
    public void AddInteraction(Interaction interaction)
    {
        _context.Interactions.Add(interaction);
        _context.SaveChanges();
    }

    // Get all interactions for a specific lead, sorted by newest first
    // AsNoTracking() makes reads faster because we don't need to track changes for read-only data
    public List<Interaction> GetInteractionsByLead(int leadId)
    {
        return _context.Interactions
            .AsNoTracking()                                    // Faster reads since we're not editing
            .Where(i => i.LeadId == leadId)                    // Only get interactions for this lead
            .OrderByDescending(i => i.InteractionDate)         // Newest interactions appear first
            .ToList();                                          // Execute the query and return results
    }
}

/*
 * FILE SUMMARY: Data/EfInteractionRepository.cs
 * 
 * This file contains the actual database logic for managing interactions using Entity Framework Core.
 * It implements the IInteractionRepository interface, providing real code for adding interactions
 * and retrieving all interactions for a specific lead (sorted newest first).
 * Like EfLeadRepository, it uses AsNoTracking() for better read performance.
 * This is the class that directly communicates with SQL Server for all interaction-related data.
 */
