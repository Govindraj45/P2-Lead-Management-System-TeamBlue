// Bring in the Lead model so we can use it in our method signatures
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Interfaces;

// An "interface" is like a contract — it lists what actions a Lead Repository MUST support
// Any class that implements this interface must provide real code for each of these methods
// The "I" prefix is a C# naming convention that means "this is an interface"
public interface ILeadRepository
{
    // Add a brand new lead to the database (Create operation)
    void AddLead(Lead lead);

    // Find and return a single lead by its ID, or null if not found (Read operation)
    Lead? GetLeadById(int id);

    // Get a list of ALL leads in the database (Read operation)
    List<Lead> GetAllLeads();

    // Save changes to an existing lead (Update operation)
    void UpdateLead(Lead lead);

    // Remove a lead from the database by its ID (Delete operation)
    void DeleteLead(int id);
}

/*
 * FILE SUMMARY: Interfaces/ILeadRepository.cs
 * 
 * This file defines the "contract" for how lead data is accessed and stored.
 * It lists five basic operations: Create, Read (one or all), Update, and Delete (CRUD).
 * The actual database code lives in a separate class (EfLeadRepository) that implements this interface.
 * This separation makes it easy to swap out the database layer without changing the rest of the app,
 * and it also makes testing much simpler since you can create a fake version for tests.
 */