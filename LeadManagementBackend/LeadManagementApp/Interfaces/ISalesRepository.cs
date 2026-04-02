using LeadManagementSystem.Models;

namespace LeadManagementSystem.Interfaces;

public interface ISalesRepository
{
    void AddSalesRep(SalesRep rep);
    List<SalesRep> GetAllReps();
    SalesRep? GetRepById(int id);
    void UpdateRep(SalesRep rep);
    void DeleteRep(int id);
}
