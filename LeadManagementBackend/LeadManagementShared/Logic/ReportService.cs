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

    // Generate Lead Analytics 
    public void ShowLeadStatusDistribution()
    {
        var stats = GetLeadStatusDistribution();

        Console.WriteLine("\n--- Lead Status Distribution ---");
        foreach (var item in stats)
        {
            Console.WriteLine($"{item.Status}: {item.Count}");
        }
    }
}

public sealed record LeadStatusStat(string Status, int Count);
