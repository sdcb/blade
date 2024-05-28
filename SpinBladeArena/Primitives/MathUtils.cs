namespace SpinBladeArena.Primitives;

public class MathUtils
{
    public static float AbsAdd(float val, float addValue)
    {
        bool isPositive = val > 0;
        return isPositive ? val + addValue : val - addValue;
    }
}
