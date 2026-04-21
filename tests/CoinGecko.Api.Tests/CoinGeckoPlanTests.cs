using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class CoinGeckoPlanTests
{
    [Fact]
    public void Plans_are_ordered_ascending_by_capability()
    {
        // Ordinal comparison must work for plan gating:
        ((int)CoinGeckoPlan.Demo).ShouldBeLessThan((int)CoinGeckoPlan.Basic);
        ((int)CoinGeckoPlan.Basic).ShouldBeLessThan((int)CoinGeckoPlan.Analyst);
        ((int)CoinGeckoPlan.Analyst).ShouldBeLessThan((int)CoinGeckoPlan.Lite);
        ((int)CoinGeckoPlan.Lite).ShouldBeLessThan((int)CoinGeckoPlan.Pro);
        ((int)CoinGeckoPlan.Pro).ShouldBeLessThan((int)CoinGeckoPlan.ProPlus);
        ((int)CoinGeckoPlan.ProPlus).ShouldBeLessThan((int)CoinGeckoPlan.Enterprise);
    }

    [Fact]
    public void Demo_is_default_integer_value_zero()
    {
        ((int)default(CoinGeckoPlan)).ShouldBe(0);
        default(CoinGeckoPlan).ShouldBe(CoinGeckoPlan.Demo);
    }
}
