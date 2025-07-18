using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Fireball;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCFireballProjectileSystem))]
public sealed partial class MCProjectileMaxRangeComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityCoordinates? Origin;

    [DataField(required: true), AutoNetworkedField]
    public float Max;

    [DataField, AutoNetworkedField]
    public bool Delete = true;
}
