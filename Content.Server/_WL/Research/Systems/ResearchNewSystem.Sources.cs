using System.Numerics;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    [Dependency] private SharedResearchConditionsSystem _researchConditions = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private void InitializeSource()
    {
        SubscribeLocalEvent<PointsSourceDebugComponent, GetServerResearchEvent>(OnDebugServerResearch);
        SubscribeLocalEvent<ResearchDetectorComponent, GetServerResearchEvent>(OnDetectorServerResearch);

        SubscribeLocalEvent<ResearchSourceComponent, GetResearchDataEvent>(OnSourceDetect);
    }

    private void OnDebugServerResearch(Entity<PointsSourceDebugComponent> ent, ref GetServerResearchEvent args)
    {
        AddResearch(ent.Comp.DebugCategory, ent.Comp.Type, ent.Comp.Value, ent.Comp.PointsType, ent.Comp.Points, ref args);
    }

    public bool CanProduce(Entity<ResearchDetectorComponent> source)
    {
        return source.Comp.Active && this.IsPowered(source, EntityManager);
    }

    private void AddResearch(
        ProtoId<ResearchCategoryPrototype> category,
        ProtoId<ResearchTypePrototype> resType,
        double resValue,
        ProtoId<ResearchPointsTypePrototype> pointsType,
        double pointsValue,
        ref GetServerResearchEvent args)
    {
        if (!args.ResearchData.ContainsKey(category))
        {
            args.ResearchData.Add(
                category,
                (new Dictionary<ProtoId<ResearchTypePrototype>, double>() {[resType] = resValue},
                 new Dictionary<ProtoId<ResearchPointsTypePrototype>, double>() {[pointsType] = pointsValue})
            );
            return;
        }

        if (args.ResearchData[category].Item1.TryAdd(resType, resValue))
            if (!args.ResearchData[category].Item2.TryAdd(pointsType, pointsValue))
                args.ResearchData[category].Item2[pointsType] = args.ResearchData[category].Item2[pointsType] + pointsValue;
    }

    private bool TryFindResearches(EntityUid ent, float range, out List<EntityUid> finded)
    {
        var entXForm = Transform(ent);
        var worldPos = _transform.GetWorldPosition(entXForm);

        var box = Box2.CenteredAround(worldPos, new Vector2(range));

        finded = new List<EntityUid>();

        foreach (var entity in _lookup.GetEntitiesIntersecting(entXForm.MapID, box))
        {
            if (HasComp<ResearchSourceComponent>(entity))
                finded.Add(entity);
        }

        return finded.Count != 0;
    }

    private void OnDetectorServerResearch(Entity<ResearchDetectorComponent> ent, ref GetServerResearchEvent args)
    {
        if (!CanProduce(ent))
            return;

        if (!TryFindResearches(ent.Owner, ent.Comp.Range, out var sources))
            return;

        var ev = new GetResearchDataEvent(ent.Owner, ent.Comp.ResearchType);
        foreach (var source in sources)
        {
            RaiseLocalEvent(source, ref ev);
        }

        foreach (var (category, (researchValue, data)) in ev.ResearchData)
        {
            var normileCoof = data.Values.Sum();

            foreach (var (type, value) in data)
            {
                var timedType = type;

                if (type != ent.Comp.ExtrimalPointsType && ent.Comp.DefaultPointsType is not null)
                    timedType = ent.Comp.DefaultPointsType.Value;

                AddResearch(category, ent.Comp.ResearchType, researchValue, type, value / normileCoof * ent.Comp.ResearchPercent, ref args);
            }
        }
    }

    private void OnSourceDetect(Entity<ResearchSourceComponent> ent, ref GetResearchDataEvent args)
    {
        if (args.ResearchData.ContainsKey(ent.Comp.Category))
            return;

        var categoryProto = ProtoMan.Index<ResearchCategoryPrototype>(ent.Comp.Category);

        if (!categoryProto.ResearchTypes.Contains(args.ResearchType))
            return;

        var typeProto = ProtoMan.Index<ResearchTypePrototype>(args.ResearchType);

        var (researchValue, poitsDict) = _researchConditions.GetCondition(ent, typeProto.Condition, args.Detector);

        if (categoryProto.PointsType is not null)
        {
            Dictionary<ProtoId<ResearchPointsTypePrototype>, double> newDict = new();

            double points = 0;

            foreach (var (type, value) in poitsDict)
            {
                if (type != categoryProto.ExtrimalPoints)
                    points += value;
                else
                    newDict.Add(type, value);
            }

            newDict.Add(categoryProto.PointsType.Value, points);

            poitsDict = newDict;
        }

        args.ResearchData.Add(ent.Comp.Category, (researchValue, poitsDict));
    }
}
