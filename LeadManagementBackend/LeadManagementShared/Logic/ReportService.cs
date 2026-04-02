// Import the data access layer and interfaces we need
using LeadManagementSystem.Data;
using LeadManagementSystem.Interfaces;

namespace LeadManagementSystem.Logic;

// This class generates reports and analytics about leads in the system
public class ReportService
{
    // The repository we use to read lead data from the database
    private readonly ILeadRepository _repo;

    // Constructor: receives the lead repository through dependency injection
    public ReportService(ILeadRepository repo)
    {
        _repo = repo;
    }

    // Count how many leads are in each status (New, Contacted, Qualified, etc.)
    // Returns a list of status names and their counts, sorted from most to least
    public List<LeadStatusStat> GetLeadStatusDistribution()
    {
        // Get all leads from the database
        var leads = _repo.GetAllLeads();

        // Group leads by their status, count each group, and sort by count (highest first)
        return leads.GroupBy(l => l.Status)
            .Select(g => new LeadStatusStat(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    // Print the lead status distribution to the console (useful for debugging or CLI tools)
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

// A simple data object that holds one row of the status report: a status name and its count
public sealed record LeadStatusStat(string Status, int Count);

/*
 * FILE SUMMARY — Logic/ReportService.cs (Shared Library)
 * This file provides reporting and analytics functionality for the Lead Management System.
 * Its main feature is calculating how many leads exist in each pipeline status (New, Contacted, etc.),
 * which powers dashboard charts and summary reports.
 * As part of the shared library, this service is used by the Reports microservice and the
 * monolithic app to generate consistent analytics data from the same business logic.
 */
