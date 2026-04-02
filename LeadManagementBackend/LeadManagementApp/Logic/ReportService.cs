using LeadManagementSystem.Data;
using LeadManagementSystem.Interfaces;

namespace LeadManagementSystem.Logic;

public class ReportService
{
    private readonly ILeadRepository _repo;

    public ReportService(ILeadRepository repo)
    {
        _repo = repo;
    }

    public List<LeadStatusStat> GetLeadStatusDistribution()
    {
        var leads = _repo.GetAllLeads();
        return leads.GroupBy(l => l.Status)
            .Select(g => new LeadStatusStat(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    public List<LeadSourceStat> GetLeadsBySource()
    {
        var leads = _repo.GetAllLeads();
        return leads.GroupBy(l => l.Source)
            .Select(g => new LeadSourceStat(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    public ConversionRateStat GetConversionRate()
    {
        var leads = _repo.GetAllLeads();
        var total = leads.Count;
        var converted = leads.Count(l => l.Status == "Converted");
        var rate = total > 0 ? Math.Round((double)converted / total * 100, 2) : 0;
        return new ConversionRateStat(total, converted, rate);
    }

    public List<SalesRepStat> GetLeadsBySalesRep()
    {
        var leads = _repo.GetAllLeads();
        return leads.Where(l => l.AssignedSalesRepId.HasValue)
            .GroupBy(l => l.AssignedSalesRepId!.Value)
            .Select(g => new SalesRepStat(
                g.Key,
                g.Count(),
                g.Count(l => l.Status == "Converted")))
            .OrderByDescending(x => x.AssignedCount)
            .ToList();
    }
}

public sealed record LeadStatusStat(string Status, int Count);
public sealed record LeadSourceStat(string Source, int Count);
public sealed record ConversionRateStat(int TotalLeads, int ConvertedLeads, double ConversionRate);
public sealed record SalesRepStat(int SalesRepId, int AssignedCount, int ConvertedCount);
