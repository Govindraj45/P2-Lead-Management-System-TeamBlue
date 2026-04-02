// Import the Interaction model so we can use it in method signatures
using LeadManagementSystem.Models;

// This interface lives in the shared Interfaces namespace so all microservices can use it
namespace LeadManagementSystem.Interfaces;

// This interface defines the "contract" for how any interaction data access class must behave
public interface IInteractionRepository
{
    // Create: save a new interaction (call, email, meeting) to the database
    void AddInteraction(Interaction interaction);

    // Read: get all interactions that belong to a specific lead
    List<Interaction> GetInteractionsByLead(int leadId);
}

/*
 * FILE SUMMARY — Interfaces/IInteractionRepository.cs (Shared Library)
 * This file defines the IInteractionRepository interface — a contract for reading and writing
 * Interaction records (calls, emails, meetings with leads).
 * It provides two operations: adding a new interaction and fetching all interactions for a given lead.
 * As part of the shared library, this interface is used by the Interactions microservice and
 * any other service that needs to work with interaction data without depending on a specific database.
 */
