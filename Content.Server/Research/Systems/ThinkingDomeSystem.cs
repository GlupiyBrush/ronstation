using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Server.Mind.Toolshed;
using Content.Server.Nutrition.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Research.Components;
using Content.Server.Storage.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Power;
using Content.Shared.Research.Components;
using Content.Shared.Storage.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server.Research.Systems;

public sealed class ThinkingDomeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ThinkingDomeComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<ThinkingDomeComponent, StorageAfterOpenEvent>(OnOpen);
        SubscribeLocalEvent<ThinkingDomeComponent, StorageAfterCloseEvent>(OnClosed);
    }

    public void OnOpen(EntityUid uid, ThinkingDomeComponent component, ref StorageAfterOpenEvent args)
    {
        StopProducingPoints(uid, component);
    }

    public void OnClosed(EntityUid uid, ThinkingDomeComponent component, ref StorageAfterCloseEvent args)
    {
        StartProducingPoints(uid, component);
    }

    public void StartProducingPoints(EntityUid uid, ThinkingDomeComponent? component = null, EntityStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref component, ref storage))
            return;

        if (component.Working)
            return;

        if (!this.IsPowered(uid, EntityManager))
            return;

        if (!IsOccupantIntelligent(uid, out _, component, storage))
            return;

        component.Working = true;
        component.NextUpdate = _timing.CurTime + component.UpdateTime;
        component.Stream = _audio.PlayPvs(component.ThinkingSound, uid)?.Entity;
    }

    public void StopProducingPoints(EntityUid uid, ThinkingDomeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.Working)
            return;

        component.Working = false;
        component.Stream = _audio.Stop(component.Stream);
    }

    public bool IsOccupantIntelligent(EntityUid uid, [NotNullWhen(true)] out EntityUid? occupant, ThinkingDomeComponent? component = null, EntityStorageComponent? storage = null)
    {
        occupant = null;

        if (!Resolve(uid, ref component, ref storage))
            return false;

        occupant = storage.Contents.ContainedEntities.FirstOrDefault();

        if (!TryComp<MindContainerComponent>(occupant, out var mind))
            return false;

        if (!mind.HasMind)
            return false;

        return true;
    }

    public void OnPowerChanged(EntityUid uid, ThinkingDomeComponent component, ref PowerChangedEvent args)
    {
        if (!args.Powered)
        {
            StopProducingPoints(uid, component);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ThinkingDomeComponent, EntityStorageComponent>();
        while (query.MoveNext(out var uid, out var component, out var storage))
        {
            if (IsOccupantIntelligent(uid, out var occupant, component, storage))
            {
                if (!component.Working)
                    StartProducingPoints(uid, component, storage);
            }
            else
            {
                StopProducingPoints(uid, component);
                continue;
            }

            if (!component.Working)
                continue;

            if (_timing.CurTime < component.NextUpdate)
                continue;
            component.NextUpdate += component.UpdateTime;

            if (!TryComp<ResearchClientComponent>(uid, out var researchComponent))
                continue;

            EntityUid? serverUid = researchComponent.Server;

            if (!TryComp<ResearchServerComponent>(serverUid, out var researchServerComponent))
                continue;

            researchServerComponent.Points += component.PointsPerSecond;
        }
    }
}
