using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Fireball;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCFireballProjectileSystem))]
public sealed partial class MCFireballProjectileComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityCoordinates? Origin;

    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    [DataField, AutoNetworkedField]
    public bool ProjectileAdjust = true;
}
