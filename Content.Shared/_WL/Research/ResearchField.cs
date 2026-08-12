using System.Linq;
using Content.Shared._WL.Types;
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

    public ResearchPoint AddPoints(double value, out double diff)
    {
        diff = Math.Min(Max, Value + value) - Value;
        Value += diff;

        return this;
    }

    public bool IsMax()
    {
        return Value == Max;
    }
}

//[Serializable, NetSerializable]
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
        var field = new RobustableArray<ResearchPoint>(sizes);

        var count = 1;
        var rank = sizes.Length;

        foreach (var size in sizes)
        {
            count *= size;
        }

        double sum = 0;

        for (int i = count - 1; i >= 0; i--)
        {
            int[] coords = new int[rank];
            double max = 0;
            var offset = i;

            for (int j = rank - 1; j >= 0; j--)
            {
                coords[j] = offset % sizes[j];
                offset /= sizes[j];

                max += researchProtos[j].GetScale(coords[j]);
            }

            sum += max;

            field.Set(coords, new ResearchPoint(max));
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

        var point = FieldArray.Get(coords.AsSpan()).AddPoints(points * _fieldScale, out var diff);
        FieldArray.Set(coords.AsSpan(), point);

        return diff / _fieldScale;
    }
}
