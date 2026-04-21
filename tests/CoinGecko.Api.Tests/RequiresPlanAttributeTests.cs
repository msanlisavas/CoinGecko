using System.Reflection;
using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class RequiresPlanAttributeTests
{
    [Fact]
    public void Attribute_targets_methods_and_classes()
    {
        var targets = typeof(RequiresPlanAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>()!
            .ValidOn;

        targets.HasFlag(AttributeTargets.Method).ShouldBeTrue();
        targets.HasFlag(AttributeTargets.Class).ShouldBeTrue();
        targets.HasFlag(AttributeTargets.Interface).ShouldBeTrue();
    }

    [Fact]
    public void Attribute_carries_required_plan()
    {
        var attr = new RequiresPlanAttribute(CoinGeckoPlan.Analyst);
        attr.Plan.ShouldBe(CoinGeckoPlan.Analyst);
    }

    [Fact]
    public void Attribute_is_not_inherited()
    {
        typeof(RequiresPlanAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>()!
            .Inherited.ShouldBeFalse("method-level gating should not propagate to overrides automatically");
    }
}
