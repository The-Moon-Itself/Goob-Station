namespace Content.Client.Atmos.Visualizers;

[RegisterComponent]
public sealed partial class PortablePumpVisualsComponent : Component
{
    [DataField("idleState", required: true)]
    public string IdleState = default!;

    [DataField("runningState", required: true)]
    public string RunningState = default!;
}
