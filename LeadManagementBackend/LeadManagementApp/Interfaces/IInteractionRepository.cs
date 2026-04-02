// Bring in the Interaction model so we can use it in our method signatures
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Interfaces;

// An "interface" is like a contract — it lists what actions an Interaction Repository MUST support
// Any class that implements this interface must provide real code for each of these methods
public interface IInteractionRepository
{
    // Save a new interaction (call, email, or meeting) to the database
    void AddInteraction(Interaction interaction);

    // Get all interactions that belong to a specific lead (found by the lead's ID)
    List<Interaction> GetInteractionsByLead(int leadId);
}

/*
 * FILE SUMMARY: Interfaces/IInteractionRepository.cs
 * 
 * This file defines the "contract" for how interaction data is accessed and stored.
 * It supports just two operations: adding a new interaction and retrieving all interactions for a lead.
 * The actual database code lives in EfInteractionRepository, which implements this interface.
 * This separation keeps the database logic isolated from the business logic,
 * making the code easier to test and maintain.
 */
