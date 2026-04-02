// Import the interface this class implements, the models, and Entity Framework Core
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Data;

// This class is the actual database implementation of ILeadRepository
// "Ef" stands for "Entity Framework" — the tool we use to talk to SQL Server
public class EfLeadRepository : ILeadRepository
{
    // Store a reference to the database context so we can use it in every method
    private readonly LeadDbContext _context;

    // Constructor: receives the database context through dependency injection
    public EfLeadRepository(LeadDbContext context)
    {
        _context = context;
    }

    // Add a new lead to the database and save immediately
    public void AddLead(Lead lead)
    {
        _context.Leads.Add(lead);
        _context.SaveChanges();
    }

    // Find a single lead by its ID, and also load its assigned sales rep and interactions
    public Lead? GetLeadById(int id)
    {
        return _context.Leads
            .Include(l => l.AssignedSalesRep)
            .Include(l => l.Interactions)
            .FirstOrDefault(l => l.LeadId == id);
    }

    // Get all leads from the database, sorted by newest first, with their sales rep info
    public List<Lead> GetAllLeads()
    {
        return _context.Leads
            .Include(l => l.AssignedSalesRep)
            .OrderByDescending(l => l.CreatedDate)
            .ToList();
    }

    // Update an existing lead's data in the database and save
    public void UpdateLead(Lead lead)
    {
        _context.Leads.Update(lead);
        _context.SaveChanges();
    }

    // Delete a lead by its ID — first check if it exists, then remove and save
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
 * FILE SUMMARY — Data/EfLeadRepository.cs (Shared Library)
 * This file implements the ILeadRepository interface using Entity Framework Core to talk to SQL Server.
 * It provides the actual database logic for creating, reading, updating, and deleting (CRUD) leads.
 * Each method uses the LeadDbContext to run queries and save changes to the Leads table.
 * As part of the shared library, this repository class is used by any microservice that needs
 * to perform lead data operations, keeping the database access code in one reusable place.
 */
