using LeadManagementSystem.Models;
namespace LeadManagementSystem.Interfaces;
public interface ILeadRepository
{
    void AddLead(Lead lead);           // Create 
    Lead? GetLeadById(int id);         // Read 
    List<Lead> GetAllLeads();          // Read 
    void UpdateLead(Lead lead);        // Update 
    void DeleteLead(int id);           // Delete 
}