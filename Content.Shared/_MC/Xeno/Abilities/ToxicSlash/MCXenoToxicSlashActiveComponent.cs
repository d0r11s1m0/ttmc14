﻿using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.ToxicSlash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoToxicSlashActiveComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan Duration;

    [ViewVariables, AutoNetworkedField]
    public int Stacks;

    [ViewVariables, AutoNetworkedField]
    public int Slashes;
}
