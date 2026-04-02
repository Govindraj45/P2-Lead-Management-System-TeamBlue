// Bring in the interface this class implements, the Lead model, and Entity Framework tools
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Data;

// This class is the REAL implementation of ILeadRepository
// It contains the actual database code for creating, reading, updating, and deleting leads
// "Ef" stands for "Entity Framework" — the tool we use to talk to SQL Server
public class EfLeadRepository : ILeadRepository
{
    // This is our database connection — we use it in every method below
    private readonly LeadDbContext _context;

    // The constructor receives the database context through "dependency injection"
    // This means the app automatically provides the database connection when this class is created
    public EfLeadRepository(LeadDbContext context)
    {
        _context = context;
    }

    // Add a new lead to the database and save it immediately
    public void AddLead(Lead lead)
    {
        _context.Leads.Add(lead);
        _context.SaveChanges();
    }

    // Find a single lead by its ID and include its related data (sales rep and interactions)
    // AsNoTracking() makes reads faster because we don't need to track changes for read-only data
    // Returns null if no lead with that ID exists
    public Lead? GetLeadById(int id)
    {
        return _context.Leads
            .AsNoTracking()
            .Include(l => l.AssignedSalesRep)   // Also load the assigned salesperson's info
            .Include(l => l.Interactions)         // Also load all interactions for this lead
            .FirstOrDefault(l => l.LeadId == id); // Find the first lead matching this ID
    }

    // Get ALL leads from the database, sorted by newest first
    // Includes the assigned sales rep info for each lead
    public List<Lead> GetAllLeads()
    {
        return _context.Leads
            .AsNoTracking()                            // Faster reads since we're not editing
            .Include(l => l.AssignedSalesRep)          // Also load each lead's salesperson
            .OrderByDescending(l => l.CreatedDate)     // Newest leads appear first
            .ToList();                                  // Execute the query and return results
    }

    // Save changes to an existing lead in the database
    public void UpdateLead(Lead lead)
    {
        _context.Leads.Update(lead);
        _context.SaveChanges();
    }

    // Remove a lead from the database by its ID
    // First checks if the lead exists — if not, it simply does nothing
    public void DeleteLead(int id)
    {
        var lead = _context.Leads.Find(id);
        if (lead != null)
        {
            _context.Leads.Remove(lead);
            _context.SaveChanges();
        }
    }
}

/*
 * FILE SUMMARY: Data/EfLeadRepository.cs
 * 
 * This file contains the actual database logic for managing leads using Entity Framework Core.
 * It implements the ILeadRepository interface, providing real code for all five CRUD operations:
 * adding, finding by ID, listing all, updating, and deleting leads.
 * It uses AsNoTracking() for read operations to improve performance, and Include() to load
 * related data (like the assigned sales rep and interactions) in a single database query.
 * This is the class that directly communicates with SQL Server for all lead-related data.
 */
