namespace Pqrs.Domain.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    bool HasTenant { get; }
}
