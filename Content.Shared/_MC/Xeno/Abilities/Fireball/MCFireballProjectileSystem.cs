using System.Numerics;
using Content.Shared._RMC14.Evasion;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Random;
using Content.Shared._RMC14.Weapons.Ranged.Prediction;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Interaction;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._MC.Xeno.Abilities.Fireball;

public sealed class MCFireballProjectileSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DeleteOnCollideComponent, StartCollideEvent>(OnDeleteOnCollideStartCollide);
        SubscribeLocalEvent<ModifyTargetOnHitComponent, ProjectileHitEvent>(OnModifyTargetOnHit);
        SubscribeLocalEvent<MCProjectileMaxRangeComponent, MapInitEvent>(OnProjectileMaxRangeMapInit);

        SubscribeLocalEvent<MCFireballProjectileComponent, EntityTerminatingEvent>(OnSpawnInRangeOnTerminatingTerminate);
    }

    private void OnDeleteOnCollideStartCollide(Entity<DeleteOnCollideComponent> ent, ref StartCollideEvent args)
    {
        if (_net.IsServer)
            QueueDel(ent);
    }

    private void OnModifyTargetOnHit(Entity<ModifyTargetOnHitComponent> ent, ref ProjectileHitEvent args)
    {
        if (!_whitelist.IsWhitelistPassOrNull(ent.Comp.Whitelist, args.Target))
            return;

        if (ent.Comp.Add is { } add)
            EntityManager.AddComponents(args.Target, add);
    }

    private void OnProjectileMaxRangeMapInit(Entity<MCProjectileMaxRangeComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Origin = _transform.GetMoverCoordinates(ent);
        Dirty(ent);
    }

    // Spawn entity in range, for example: Spawn pyrogen fire
    private void OnSpawnInRangeOnTerminatingTerminate(Entity<MCFireballProjectileComponent> ent, ref EntityTerminatingEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp(ent, out TransformComponent? transform))
            return;

        if (TerminatingOrDeleted(transform.ParentUid))
            return;

        var coordinates = transform.Coordinates;
        if (ent.Comp.ProjectileAdjust &&
            ent.Comp.Origin is { } origin &&
            coordinates.TryDelta(EntityManager, _transform, origin, out var delta) &&
            delta.Length() > 0)
        {
            coordinates = coordinates.Offset(delta.Normalized() / -2);
        }

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (!CanPlaceFire(coordinates))
                    continue;

                if (!_interaction.InRangeUnobstructed(ent.Owner, coordinates, ent.Comp.Range))
                    continue;

                var spawn = SpawnAtPosition("MCTileFireViolet", coordinates);
                _hive.SetSameHive(ent.Owner, spawn);
            }
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
