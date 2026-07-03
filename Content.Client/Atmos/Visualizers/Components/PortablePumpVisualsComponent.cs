namespace Content.Client.Atmos.Visualizers;

/// <summary>
/// Holds 2 pairs of states. The idle/running pair controls animation, while
/// the ready / full pair controls the color of the light.
/// </summary>
[RegisterComponent]
public sealed partial class PortablePumpVisualsComponent : Component
{
    [DataField("idleState", required: true)]
    public string IdleState = default!;

    [DataField("runningState", required: true)]
    public string RunningState = default!;
}
