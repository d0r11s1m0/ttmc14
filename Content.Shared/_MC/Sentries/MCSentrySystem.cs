using Content.Shared._MC.Areas;
using Content.Shared._MC.Chat;
using Content.Shared._MC.Damage.Integrity.Systems;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Sentries;

public sealed class MCSentrySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    [Dependency] private readonly MCAreasSystem _mcArea = default!;
    [Dependency] private readonly MCIntegritySystem _mcIntegrity = default!;
    [Dependency] private readonly MCSharedRadioSystem _mcRadio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSentryComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MCSentryComponent, DestructionEventArgs>(OnDestruction);
    }

    private void OnDamageChanged(Entity<MCSentryComponent> entity, ref DamageChangedEvent args)
    {
        if (!entity.Comp.AlertMode)
            return;

        if (!args.DamageIncreased)
            return;

        if (entity.Comp.AlertDamageNextTime > _gameTiming.CurTime)
            return;

        entity.Comp.AlertDamageNextTime = _gameTiming.CurTime + entity.Comp.AlertDamageDelay;

        var message = Loc.GetString("mc-sentry-damage-alert",
            ("name", MetaData(entity).EntityName),
            ("coordsMessage", _mcArea.GetAreaCoordsMessage(entity)),
            ("integrityMessage", _mcIntegrity.GetDamageMessage(entity.Owner, "Destruction")));

        _mcRadio.SendRadioMessage(entity, message, entity.Comp.AlertChannel, entity);
    }

    private void OnDestruction(Entity<MCSentryComponent> entity, ref DestructionEventArgs args)
    {
        var message = Loc.GetString("mc-sentry-destroyed-alert", ("name", MetaData(entity).EntityName), ("coordsMessage", _mcArea.GetAreaCoordsMessage(entity)));
        _mcRadio.SendRadioMessage(entity, message, entity.Comp.AlertChannel, entity);
    }
}
