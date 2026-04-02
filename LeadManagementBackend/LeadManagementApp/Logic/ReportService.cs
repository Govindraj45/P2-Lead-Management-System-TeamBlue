// These are the libraries this file needs to access the database and its interfaces
using LeadManagementSystem.Data;
using LeadManagementSystem.Interfaces;

// This file belongs to the "Logic" folder — it generates analytics and reports
namespace LeadManagementSystem.Logic;

// This class generates reports and statistics about leads in the system
public class ReportService
{
    // The repository lets us read lead data from the database
    private readonly ILeadRepository _repo;

    // The constructor receives the repository automatically (Dependency Injection)
    public ReportService(ILeadRepository repo)
    {
        _repo = repo;
    }

    // Returns how many leads are in each status (e.g., 10 New, 5 Contacted, 3 Converted)
    public List<LeadStatusStat> GetLeadStatusDistribution()
    {
        // Get all leads from the database
        var leads = _repo.GetAllLeads();
        // Group them by status, count each group, and sort by count (highest first)
        return leads.GroupBy(l => l.Status)
            .Select(g => new LeadStatusStat(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    // Returns how many leads came from each source (e.g., 8 from Website, 4 from Referral)
    public List<LeadSourceStat> GetLeadsBySource()
    {
        var leads = _repo.GetAllLeads();
        // Group leads by their source and count each group
        return leads.GroupBy(l => l.Source)
            .Select(g => new LeadSourceStat(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    // Calculates what percentage of all leads have been converted to customers
    public ConversionRateStat GetConversionRate()
    {
        var leads = _repo.GetAllLeads();
        var total = leads.Count;
        // Count how many leads have the "Converted" status
        var converted = leads.Count(l => l.Status == "Converted");
        // Calculate the percentage (avoid dividing by zero if there are no leads)
        var rate = total > 0 ? Math.Round((double)converted / total * 100, 2) : 0;
        return new ConversionRateStat(total, converted, rate);
    }

    // Returns how many leads each sales rep is handling and how many they converted
    public List<SalesRepStat> GetLeadsBySalesRep()
    {
        var leads = _repo.GetAllLeads();
        // Only include leads that are assigned to a sales rep
        return leads.Where(l => l.AssignedSalesRepId.HasValue)
            // Group by sales rep ID
            .GroupBy(l => l.AssignedSalesRepId!.Value)
            // For each rep, count total assigned and total converted
            .Select(g => new SalesRepStat(
                g.Key,
                g.Count(),
                g.Count(l => l.Status == "Converted")))
            .OrderByDescending(x => x.AssignedCount)
            .ToList();
    }
}

// These "record" types are simple data containers used to return report results
// A record automatically creates a class with the listed properties

// Holds a status name and how many leads have that status
public sealed record LeadStatusStat(string Status, int Count);
// Holds a source name and how many leads came from that source
public sealed record LeadSourceStat(string Source, int Count);
// Holds the total leads, converted leads, and the conversion percentage
public sealed record ConversionRateStat(int TotalLeads, int ConvertedLeads, double ConversionRate);
// Holds a sales rep's ID, how many leads they have, and how many they converted
public sealed record SalesRepStat(int SalesRepId, int AssignedCount, int ConvertedCount);

/*
 * FILE SUMMARY: ReportService.cs
 *
 * This file generates all the analytics and reports for the Lead Management System.
 * It provides four key reports: leads grouped by status, leads grouped by source,
 * the overall conversion rate, and performance stats per sales representative.
 * These reports power the dashboard charts and analytics pages in the frontend.
 * The record types at the bottom define the shape of data each report returns.
 */
