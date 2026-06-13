using Knights.Domain.Dashboards;

namespace Knights.Domain.Tests.Dashboards;

public sealed class DashboardTests
{
    [Fact]
    public void Create_with_valid_name_assigns_owner_and_slug()
    {
        var ownerId = Guid.NewGuid();

        var dashboard = Dashboard.Create(Guid.NewGuid(), ownerId, "Identity Overview");

        Assert.Equal(ownerId, dashboard.OwnerUserId);
        Assert.Equal("identity-overview", dashboard.Slug);
    }

    [Fact]
    public void AddWidget_with_invalid_json_rejects_configuration()
    {
        var dashboard = Dashboard.Create(Guid.NewGuid(), Guid.NewGuid(), "Operations");
        var command = new AddDashboardWidget("Users", WidgetType.Grid, "users", "not-json", "{}", 0, 0, 6, 4);

        Assert.ThrowsAny<System.Text.Json.JsonException>(() => dashboard.AddWidget(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void AddWidget_with_width_outside_grid_rejects_layout(int width)
    {
        var dashboard = Dashboard.Create(Guid.NewGuid(), Guid.NewGuid(), "Operations");
        var command = new AddDashboardWidget("Users", WidgetType.Grid, "users", "{}", "{}", 0, 0, width, 4);

        Assert.Throws<ArgumentOutOfRangeException>(() => dashboard.AddWidget(command));
    }
}
