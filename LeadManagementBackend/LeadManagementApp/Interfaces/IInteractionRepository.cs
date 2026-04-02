using LeadManagementSystem.Models;

namespace LeadManagementSystem.Interfaces;

public interface IInteractionRepository
{
    void AddInteraction(Interaction interaction);
    List<Interaction> GetInteractionsByLead(int leadId);
}
