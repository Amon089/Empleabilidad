using System;
using Pqrs.Infrastructure;
using Xunit;

namespace Pqrs.UnitTests;

public class TenantResolverTests
{
    [Fact]
    public void TenantContext_InitialState_HasNoTenant()
    {
        var context = new TenantContext();
        Assert.False(context.HasTenant);
        Assert.Equal(Guid.Empty, context.TenantId);
    }

    [Fact]
    public void TenantContext_SetTenantId_UpdatesTenantIdAndHasTenant()
    {
        var context = new TenantContext();
        var expectedId = Guid.NewGuid();

        context.SetTenantId(expectedId);

        Assert.True(context.HasTenant);
        Assert.Equal(expectedId, context.TenantId);
    }
}
