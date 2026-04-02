using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Data;

public class EfLeadRepository : ILeadRepository
{
    private readonly LeadDbContext _context;

    public EfLeadRepository(LeadDbContext context)
    {
        _context = context;
    }

    public void AddLead(Lead lead)
    {
        _context.Leads.Add(lead);
        _context.SaveChanges();
    }

    public Lead? GetLeadById(int id)
    {
        return _context.Leads
            .Include(l => l.AssignedRep)
            .Include(l => l.Interactions)
            .FirstOrDefault(l => l.LeadId == id);
    }

    public List<Lead> GetAllLeads()
    {
        return _context.Leads
            .Include(l => l.AssignedRep)
            .OrderByDescending(l => l.CreatedDate)
            .ToList();
    }

    public void UpdateLead(Lead lead)
    {
        _context.Leads.Update(lead);
        _context.SaveChanges();
    }

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
