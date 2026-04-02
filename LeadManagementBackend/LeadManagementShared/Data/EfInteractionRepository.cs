// Import the interface this class implements, the models, and Entity Framework Core
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Data;

// This class is the actual database implementation of IInteractionRepository
// "Ef" stands for "Entity Framework" — the tool we use to talk to SQL Server
public class EfInteractionRepository : IInteractionRepository
{
    // Store a reference to the database context so we can use it in every method
    private readonly LeadDbContext _context;

    // Constructor: receives the database context through dependency injection
    public EfInteractionRepository(LeadDbContext context)
    {
        _context = context;
    }

    // Add a new interaction (call, email, meeting) to the database and save immediately
    public void AddInteraction(Interaction interaction)
    {
        _context.Interactions.Add(interaction);
        _context.SaveChanges();
    }

    // Get all interactions for a specific lead, sorted by newest first
    public List<Interaction> GetInteractionsByLead(int leadId)
    {
        return _context.Interactions
            .Where(i => i.LeadId == leadId)
            .OrderByDescending(i => i.InteractionDate)
            .ToList();
    }
}

/*
 * FILE SUMMARY — Data/EfInteractionRepository.cs (Shared Library)
 * This file implements the IInteractionRepository interface using Entity Framework Core.
 * It provides the actual database logic for adding new interactions and fetching all
 * interactions for a given lead, sorted by date (newest first).
 * As part of the shared library, this repository is reused by the Interactions microservice
 * and any other service that needs to read or write interaction records.
 */
