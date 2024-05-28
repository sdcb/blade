using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Tests;

public class BladeTest
{
    [Fact]
    public void DefaultBladeScore_ShouldBe_1()
    {
        Blade def = new();
        Assert.Equal(1, def.Score);
    }
}
