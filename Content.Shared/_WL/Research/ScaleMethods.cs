namespace Content.Shared._WL.Research;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ScaleMethod
{
    [DataField]
    public int min = 1;

    [DataField]
    public int max = 100;

    internal abstract double GetModifier(double x); // D [0, 1]; E [0, 1]. 0 => 0, 1 => 1.

    public double GetModifier(int x, int size)
    {
        double sized = Math.Clamp((double)x/(double)size, 0, 1);

        return GetModifier(sized) * max + min;
    }
}

public sealed partial class ConstMethod : ScaleMethod
{
    internal override double GetModifier(double x)
    {
        return 0.5;
    }
}

public sealed partial class LinearMethod : ScaleMethod
{
    internal override double GetModifier(double x)
    {
        return x;
    }
}

public sealed partial class ExpMethod : ScaleMethod
{
    [DataField(required: true)]
    public double dbase;

    internal override double GetModifier(double x)
    {
        if (dbase <= 1)
            return 0;

        return (Math.Pow(dbase, x) - 1)/(dbase - 1);
    }
}
