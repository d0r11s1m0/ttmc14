using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Xenonids.GasToggle;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared._MC.Xeno.Abilities.Fireball;

public sealed class MCXenoFireballSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly RMCProjectileSystem _rmcProjectile = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoFireballComponent, MCXenoFireballActionEvent>(OnFireball);
        SubscribeLocalEvent<MCXenoFireballComponent, MCXenoFireballDoAfterEvent>(OnFireballDoAfter);
    }

    private void OnFireball(Entity<MCXenoFireballComponent> xeno, ref MCXenoFireballActionEvent args)
    {
        var source = _transform.GetMapCoordinates(xeno);
        var target = _transform.ToMapCoordinates(args.Target);
        if (source.MapId != target.MapId)
            return;

        args.Handled = true;

        if (!_xenoPlasma.HasPlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        var direction = target.Position - source.Position;
        if (direction.Length() > xeno.Comp.Range)
            target = target.Offset(direction.Normalized() * xeno.Comp.Range);

        _audio.PlayPvs(xeno.Comp.Sound, xeno);

        var ev = new MCXenoFireballDoAfterEvent { Coordinates = target, };
        var doAfter = new DoAfterArgs(EntityManager, xeno, xeno.Comp.Delay, ev, xeno, args.Action) { BreakOnMove = true };
        if (_doAfter.TryStartDoAfter(doAfter))
            args.Handled = true;
    }

    private void OnFireballDoAfter(Entity<MCXenoFireballComponent> xeno, ref MCXenoFireballDoAfterEvent args)
    {
        if (args.Target is not { } action)
            return;

        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        if (_net.IsClient)
            return;

        var source = _transform.GetMapCoordinates(xeno);
        if (source.MapId != args.Coordinates.MapId)
            return;

        var direction = args.Coordinates.Position - source.Position;
        var projectile = Spawn(xeno.Comp.Projectile, source);
        _hive.SetSameHive(xeno.Owner, projectile);

        var max = EnsureComp<ProjectileMaxRangeComponent>(projectile);
        _rmcProjectile.SetMaxRange((projectile, max), direction.Length());
        _gun.ShootProjectile(projectile, direction, Vector2.Zero, xeno, xeno, speed: 7.5f);

        foreach (var (actionId, actionComp) in _actions.GetActions(xeno))
        {
            if (actionComp.BaseEvent is MCXenoFireballActionEvent)
                _actions.SetIfBiggerCooldown(actionId, xeno.Comp.Cooldown);
        }
    }
}
