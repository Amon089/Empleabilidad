using System;
using Pqrs.Domain.Interfaces;

namespace Pqrs.Infrastructure;

public interface ITenantSetter
{
    void SetTenantId(Guid tenantId);
}

public class TenantContext : ITenantContext, ITenantSetter
{
    private Guid? _tenantId;

    public Guid TenantId => _tenantId ?? Guid.Empty;
    public bool HasTenant => _tenantId.HasValue && _tenantId.Value != Guid.Empty;

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}
