using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeadManagementSystem.Models;

public class Lead 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LeadId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string Status { get; set; } = "New"; // New, Contacted, Qualified, Unqualified, Converted
    public string Source { get; set; } = "Website";
    public string Priority { get; set; } = "Medium";
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }

    // Foreign Key to Users (SalesRep role user)
    public int? AssignedSalesRepId { get; set; }
    public virtual User? AssignedSalesRep { get; set; }

    // Relationship: One Lead can have many Interactions
    public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
}
