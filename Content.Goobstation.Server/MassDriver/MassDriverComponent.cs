
using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Server.MassDriver;

[RegisterComponent]
public sealed partial class MassDriverComponent : Component
{
    /// <summary>
    /// How forceful the mass driver should launch items
    /// </summary>
    [DataField]
    public float Power = 10;
    /// <summary>
    /// How far the mass driver should launch items
    /// </summary>
    [DataField]
    public float DriveRange = 50;
    /// <summary>
    /// How much power the mass driver should draw per item
    /// </summary>
    [DataField]
    public int PowerPerObject = 100;
    /// <summary>
    /// How many items the driver can launch in one throw.
    /// </summary>
    [DataField]
    public int ObjectLimit = 20;
    /// <summary>
    /// How long to wait in seconds until the massi driver can be pulsed again
    /// </summary>
    [DataField]
    public float Cooldown = 0.5f;

    /// <summary>
    /// Which direction to fire in offset from the direction of the mass driver.
    /// </summary>
    [DataField]
    public Angle TargetAngle = new Angle(0);

    /// <summary>
    /// When the driver is ready to be fired
    /// </summary>
    public TimeSpan ReadyWhen = TimeSpan.Zero;

    /// <summary>
    /// The signal port to activate the driver
    /// </summary>
    [DataField("launchPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string LaunchPort = "MassDriverLaunch";
}
