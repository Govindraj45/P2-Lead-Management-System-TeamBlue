// Import the Lead model so we can use it in method signatures
using LeadManagementSystem.Models;

// This interface lives in the shared Interfaces namespace so all microservices can use it
namespace LeadManagementSystem.Interfaces;

// This interface defines the "contract" for how any lead data access class must behave
// Any class that implements this must provide these 5 methods (CRUD operations)
public interface ILeadRepository
{
    // Create: save a new lead to the database
    void AddLead(Lead lead);

    // Read: find one lead by its ID number
    Lead? GetLeadById(int id);

    // Read: get a list of all leads
    List<Lead> GetAllLeads();

    // Update: save changes to an existing lead
    void UpdateLead(Lead lead);

    // Delete: remove a lead by its ID number
    void DeleteLead(int id);
}

/*
 * FILE SUMMARY — Interfaces/ILeadRepository.cs (Shared Library)
 * This file defines the ILeadRepository interface — a contract that lists all the ways
 * code can read and write Lead data (Create, Read, Update, Delete).
 * It does NOT contain any actual database code; that lives in the implementing class (EfLeadRepository).
 * This separation follows the Dependency Inversion principle so microservices depend on the
 * interface, not on a specific database technology, making the system easier to test and change.
 */