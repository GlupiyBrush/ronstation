using System.Text;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Chemistry.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Prototypes;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared.Chemistry.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Server.Chemistry.EntitySystems;

public sealed class SolutionSorptionSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    public readonly HashSet<EntityUid> ActiveEntities = new HashSet<EntityUid>();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SolutionSorptionComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<SolutionSorptionComponent, ComponentShutdown>(OnComponentShutdown);
    }

    public void OnComponentStartup(EntityUid uid, SolutionSorptionComponent component, ComponentStartup args)
    {
        if (!ComponentCheck(uid))
            return;
        ActiveEntities.Add(uid);
    }

    public void OnComponentShutdown(EntityUid uid, SolutionSorptionComponent component, ComponentShutdown args)
    {
        if (!ComponentCheck(uid))
            return;
        ActiveEntities.Remove(uid);
    }

    public bool ComponentCheck(EntityUid uid)
    {
        if (!EntityManager.TryGetComponent<SolutionContainerManagerComponent>(uid, out var solutionContainer))
            return false;
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (ActiveEntities.Count == 0) return;

        var query = GetEntityQuery<TransformComponent>();
        foreach (var uid in ActiveEntities)
        {
            if (!query.TryGetComponent(uid, out var xform) || xform.MapID == MapId.Nullspace)
                continue;

            var atmos = _atmosphere.GetContainingMixture(uid, excite: false);

            if (atmos == null)
                continue;

            HandleAtmos(uid, atmos);
        }
    }

    private void HandleAtmos(EntityUid uid, GasMixture atmos)
    {
        EntityManager.TryGetComponent<SolutionSorptionComponent>(uid, out var targetComponent);
        EntityManager.TryGetComponent<SolutionContainerManagerComponent>(uid, out var solutionManager);

        if (solutionManager == null || solutionManager.Solutions == null)
            return;

        for (var i = 0; i < Atmospherics.TotalNumberOfGases; i++)
        {
            var gas = _atmosphere.GetGas(i);

            if (atmos?[i] == 0)
                continue;

            if (gas.Reagent is not { } gasSolution)
                continue;
        }

        foreach (var name in solutionManager.Containers)
        {
            //Log.Info(name);
        }
    }
}
