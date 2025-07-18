using System.Numerics;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Maps;
using Content.Shared.Tag;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._MC.Xeno.Abilities.Inferno;

public sealed class MCXenoInfernoSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    private readonly HashSet<Entity<MobStateComponent>> _receivers = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoInfernoComponent, MCXenoInfernoActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoInfernoComponent, MCXenoInfernoDoAfterEvent>(OnXenoInfernoDoAfter);
    }

    private void OnAction(Entity<MCXenoInfernoComponent> xeno, ref MCXenoInfernoActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_xenoPlasma.HasPlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        var doAfter = new DoAfterArgs(EntityManager, xeno, xeno.Comp.InfernoDelay, new MCXenoInfernoDoAfterEvent(), xeno, xeno)
        {
            BreakOnMove = true,
            BreakOnDamage = false,
            ForceVisible = true,
            CancelDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            args.Handled = true;
    }

    private void OnXenoInfernoDoAfter(Entity<MCXenoInfernoComponent> xeno, ref MCXenoInfernoDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        if (!TryComp(xeno, out TransformComponent? xform))
            return;

        args.Handled = true;

        _audio.PlayPvs(xeno.Comp.Sound, xeno);
        if (_net.IsServer && xeno.Comp.SelfEffect is not null)
            SpawnAttachedTo(xeno.Comp.SelfEffect, xeno.Owner.ToCoordinates());

        _receivers.Clear();
        _entityLookup.GetEntitiesInRange(xform.Coordinates, xeno.Comp.Range, _receivers);

        var center = xform.Coordinates;

        for (var x = -2; x <= 2; x++)
        {
            for (var y = -2; y <= 2; y++)
            {
                var offsetPosition = center.Offset(new Vector2(x, y));

                if (!CanPlaceFire(offsetPosition))
                    continue;

                if (!_interaction.InRangeUnobstructed(xeno.Owner, offsetPosition, xeno.Comp.Range))
                    continue;

                Spawn("MCTileFireViolet", offsetPosition);
            }
        }

        foreach (var receiver in _receivers)
        {
            if (_mobState.IsDead(receiver))
                continue;

            if (!_xeno.CanAbilityAttackTarget(xeno, receiver))
                continue;

            _damageable.TryChangeDamage(
                receiver,
                _xeno.TryApplyXenoSlashDamageMultiplier(receiver, xeno.Comp.Damage),
                origin: xeno,
                tool: xeno);
        }
    }

    private bool CanPlaceFire(EntityCoordinates coords)
    {
        if (_transform.GetGrid(coords) is not { } gridId ||
            !TryComp<MapGridComponent>(gridId, out var grid))
            return false;

        var tile = _mapSystem.TileIndicesFor(gridId, grid, coords);
        return !_turf.IsTileBlocked(gridId, tile, Impassable | MidImpassable | HighImpassable, grid);
    }
}
