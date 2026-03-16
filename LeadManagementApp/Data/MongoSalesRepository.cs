using LeadManagementSystem.Models;
using LeadManagementSystem.Interfaces;
using MongoDB.Driver;

namespace LeadManagementSystem.Data;

public class MongoSalesRepository : ISalesRepository
{
    private readonly MongoDbContext _context;
    private readonly MongoSequenceService _sequenceService;

    public MongoSalesRepository(MongoDbContext context, MongoSequenceService sequenceService)
    {
        _context = context;
        _sequenceService = sequenceService;
    }

    public void AddSalesRep(SalesRep rep)
    {
        if (rep.RepId <= 0)
        {
            rep.RepId = _sequenceService.GetNextValue("rep_id");
        }

        _context.SalesRepresentatives.InsertOne(rep);
    }

    public List<SalesRep> GetAllReps()
    {
        return _context.SalesRepresentatives.Find(_ => true).ToList();
    }

    public SalesRep? GetRepById(int id)
    {
        return _context.SalesRepresentatives.Find(x => x.RepId == id).FirstOrDefault();
    }

    public void UpdateRep(SalesRep rep)
    {
        _context.SalesRepresentatives.ReplaceOne(x => x.RepId == rep.RepId, rep);
    }

    public void DeleteRep(int id)
    {
        _context.SalesRepresentatives.DeleteOne(x => x.RepId == id);
    }
}
