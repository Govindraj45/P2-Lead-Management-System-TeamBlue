using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Data;

public class EfSalesRepository : ISalesRepository
{
    private readonly LeadDbContext _context;

    public EfSalesRepository(LeadDbContext context)
    {
        _context = context;
    }

    public void AddSalesRep(SalesRep rep)
    {
        _context.SalesRepresentatives.Add(rep);
        _context.SaveChanges();
    }

    public List<SalesRep> GetAllReps()
    {
        return _context.SalesRepresentatives.ToList();
    }

    public SalesRep? GetRepById(int id)
    {
        return _context.SalesRepresentatives.Find(id);
    }

    public void UpdateRep(SalesRep rep)
    {
        _context.SalesRepresentatives.Update(rep);
        _context.SaveChanges();
    }

    public void DeleteRep(int id)
    {
        var rep = _context.SalesRepresentatives.Find(id);
        if (rep != null)
        {
            _context.SalesRepresentatives.Remove(rep);
            _context.SaveChanges();
        }
    }
}
