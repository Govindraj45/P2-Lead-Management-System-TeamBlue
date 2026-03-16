using Microsoft.Data.SqlClient;
using System.Data;
using LeadManagementSystem.Models;
using LeadManagementSystem.Interfaces;

namespace LeadManagementSystem.Data;

public class LeadRepository : ILeadRepository
{
    // Connection string provided by Member 1
    private readonly string _connectionString = "Server=localhost;Database=CRM_LeadManagement;User Id=sa;Password=Nikhil@1234;TrustServerCertificate=True";

    public void AddLead(Lead lead)
    {
        using SqlConnection conn = new SqlConnection(_connectionString);
        // Parameterized query to prevent SQL Injection 
        string sql = "INSERT INTO Leads (Name, Email, Phone, Company, Status, Source, Priority, CreatedDate) " +
                     "VALUES (@Name, @Email, @Phone, @Company, @Status, @Source, @Priority, @CreatedDate)";

        using SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = lead.Name;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = (object?)lead.Email ?? DBNull.Value;
        cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = (object?)lead.Phone ?? DBNull.Value;
        cmd.Parameters.Add("@Company", SqlDbType.NVarChar).Value = (object?)lead.Company ?? DBNull.Value;
        cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = lead.Status;
        cmd.Parameters.Add("@Source", SqlDbType.NVarChar).Value = lead.Source;
        cmd.Parameters.Add("@Priority", SqlDbType.NVarChar).Value = lead.Priority;
        cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = lead.CreatedDate;

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public Lead? GetLeadById(int id)
    {
        using SqlConnection conn = new SqlConnection(_connectionString);
        string sql = "SELECT * FROM Leads WHERE LeadId = @Id";
        using SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Lead {
                LeadId = (int)reader["LeadId"],
                Name = reader["Name"].ToString() ?? "",
                Status = reader["Status"].ToString() ?? ""
            };
        }
        return null;
    }

    public List<Lead> GetAllLeads()
    {
        List<Lead> leads = new List<Lead>();
        using LeadDbContext context = new LeadDbContext(); // Using EF Core for simple List [cite: 58]
        return context.Leads.ToList();
    }

    public void UpdateLead(Lead lead)
    {
        using LeadDbContext context = new LeadDbContext();
        context.Leads.Update(lead);
        context.SaveChanges();
    }

    public void DeleteLead(int id)
    {
        using LeadDbContext context = new LeadDbContext();
        var lead = context.Leads.Find(id);
        if (lead != null)
        {
            context.Leads.Remove(lead);
            context.SaveChanges();
        }
    }
}
