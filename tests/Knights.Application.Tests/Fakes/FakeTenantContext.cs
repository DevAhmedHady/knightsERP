namespace Knights.Application.Tests.Fakes;

using Knights.Application.Common.Interfaces;

public sealed class FakeTenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
}
