using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;
using MongoDB.Driver;

namespace LeadManagementSystem.Data;

public class MongoLeadRepository : ILeadRepository
{
    private readonly MongoDbContext _context;
    private readonly MongoSequenceService _sequenceService;

    public MongoLeadRepository(MongoDbContext context, MongoSequenceService sequenceService)
    {
        _context = context;
        _sequenceService = sequenceService;
    }

    public void AddLead(Lead lead)
    {
        if (lead.LeadId <= 0)
        {
            lead.LeadId = _sequenceService.GetNextValue("lead_id");
        }

        _context.Leads.InsertOne(lead);
    }

    public Lead? GetLeadById(int id)
    {
        return _context.Leads.Find(x => x.LeadId == id).FirstOrDefault();
    }

    public List<Lead> GetAllLeads()
    {
        return _context.Leads.Find(_ => true).SortByDescending(x => x.CreatedDate).ToList();
    }

    public void UpdateLead(Lead lead)
    {
        _context.Leads.ReplaceOne(x => x.LeadId == lead.LeadId, lead);
    }

    public void DeleteLead(int id)
    {
        _context.Leads.DeleteOne(x => x.LeadId == id);
    }
}
