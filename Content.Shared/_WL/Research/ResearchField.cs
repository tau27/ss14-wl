using System.Linq;
usnig Content.Shared._WL.Types;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared._WL.Research;

[Serializable, NetSerializable]
public struct ResearchPoint
{
    public readonly double Max;
    public double Value { get; private set; }

    public ResearchPoint(double max)
    {
        Max = max;
        Value = 0;
    }

    public double AddPoints(double value)
    {
        var diff = Math.Min(Max, Value + value) - Value;
        Value = Math.Min(Max, Value + value);

        return diff;
    }

    public bool IsMax()
    {
        return Value == Max;
    }
}

[Serializable, NetSerializable]
public sealed partial class ResearchField
{
    public int Rank { get; private set; }

    public double Max { get; private set; }

    public ProtoId<ResearchTypePrototype>[] ResearchTypes { get; private set; }

    private RobustableArray<ResearchPoint> FieldArray;

    private double _fieldScale;

    public ResearchField(ProtoId<ResearchTypePrototype>[] researchTypes, double maxPoints)
    {
        int[] sizes = new int[researchTypes.Length];
        ResearchTypePrototype[] typesProto = new ResearchTypePrototype[researchTypes.Length];
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        for (int i = 0; i < researchTypes.Length; i++)
        {
            typesProto[i] = protoMan.Index<ResearchTypePrototype>(researchTypes[i]);
            sizes[i] = typesProto[i].Size;
        }

        (FieldArray, var fieldSum) = GenField(typesProto, sizes);

        ResearchTypes = researchTypes;

        Rank = researchTypes.Length;

        Max = maxPoints;

        _fieldScale = fieldSum / maxPoints;
    }

    private (RobustableArray<ResearchPoint>, double) GenField(ResearchTypePrototype[] researchProtos, ReadOnlySpan<int> sizes)
    {
        var field = RobustableArray<ResearchPoint>(sizes);

        var count = 1;
        var rank = sizes.Length;

        foreach (var size in sizes)
        {
            count *= size;
        }

        double sum = 0;

        for (int i = count; i > 0; i--)
        {
            int[] coords = new int[rank];
            var timedCount = count;
            double max = 1;

            for (int j = 0; j < rank; j++)
            {
                timedCount /= sizes[j];
                coords[j] = i % (count/timedCount);

                max += researchProtos[j].GetScale(coords[j]);
            }

            sum += max;

            field.SetValue(new ResearchPoint(max), coords);
        }

        return (field, sum);
    }

    public double ResearchData(Dictionary<ProtoId<ResearchTypePrototype>, double> researchData, double points)
    {
        int[] coords = new int[Rank];
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        for (int i = 0; i < Rank; i++)
        {
            if (researchData.TryGetValue(ResearchTypes[i], out var value))
            {
                var type = protoMan.Index(ResearchTypes[i]);
                coords[i] = (int)Math.Clamp((value - type.MinValue) / type.MaxValue * type.Size, 0, type.Size - 1);
            }
            else
            {
                coords[i] = 0;
            }
        }

        if (FieldArray.Get(coords.AsSpan()) is not ResearchPoint point)
            return 0;

        return point.AddPoints(points);
    }

    /*
    public bool Equals(ResearchField? other)
    {
        if (other == null ||
                Rank != other.Rank ||
                ResearchTypes != other.ResearchTypes)
            return false;

        return true;
    }
    */
}
