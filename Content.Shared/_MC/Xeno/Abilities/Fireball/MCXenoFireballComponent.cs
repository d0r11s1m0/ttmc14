using Content.Shared.FixedPoint;
using Content.Shared.MC.Xeno.Abilities.Fireball;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.MC.Xeno.Abilities.Fireball;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoFireballSystem))]
public sealed partial class MCXenoFireballComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Range = 8;

    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaCost = 50;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.6);

    [DataField, AutoNetworkedField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public EntProtoId Projectile = "MCXenoProjectileFireball";

    [DataField, AutoNetworkedField]
    public SoundSpecifier ShootSound = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/fireball.ogg");
}
