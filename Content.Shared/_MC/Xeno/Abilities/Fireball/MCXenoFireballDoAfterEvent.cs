using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.MC.Xeno.Abilities.Fireball;

[Serializable, NetSerializable]
public sealed partial class MCXenoFireballDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public MapCoordinates Coordinates;
}
