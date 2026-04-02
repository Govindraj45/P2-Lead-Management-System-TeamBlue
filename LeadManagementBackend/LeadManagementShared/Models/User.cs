using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeadManagementSystem.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!; // SalesRep, SalesManager, Admin
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation: Leads assigned to this user (when Role=SalesRep)
    public virtual ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
}
