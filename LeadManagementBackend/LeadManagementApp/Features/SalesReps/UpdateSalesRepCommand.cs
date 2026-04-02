using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Models;

namespace LeadManagementSystem.Features.SalesReps;

public sealed record UpdateSalesRepCommand(
    int RepId,
    string Name,
    string Email,
    string Department);

public sealed class UpdateSalesRepHandler
{
    private readonly ISalesRepository _repository;

    public UpdateSalesRepHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public Task<OperationResult> HandleAsync(UpdateSalesRepCommand request)
    {
        var existing = _repository.GetRepById(request.RepId);
        if (existing is null)
        {
            return Task.FromResult(OperationResult.Fail("Sales representative not found."));
        }

        var rep = new SalesRep
        {
            RepId = request.RepId,
            Name = request.Name,
            Email = request.Email,
            Department = request.Department,
            AssignedLeads = existing.AssignedLeads
        };

        _repository.UpdateRep(rep);
        return Task.FromResult(OperationResult.Ok("Sales representative updated successfully."));
    }
}
