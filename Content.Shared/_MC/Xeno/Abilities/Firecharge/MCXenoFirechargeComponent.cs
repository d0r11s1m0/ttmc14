using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Firecharge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoFirechargeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MaxDistance = 3;

    [DataField, AutoNetworkedField]
    public int Strength = 45;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? HitSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_pounce.ogg");
}
