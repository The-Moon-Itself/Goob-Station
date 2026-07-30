using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.TransferValve.Components;

[RegisterComponent]
public sealed partial class TransferValveComponent : Component
{
    /// <summary>
    /// The tank attached to the right side of the ttv
    /// </summary>
    [DataField(required: true)]
    public ItemSlot Tank1Slot = new();

    /// <summary>
    /// The tank attached to the left side of the ttv
    /// </summary>
    [DataField(required: true)]
    public ItemSlot Tank2Slot = new();

    [DataField]
    public SoundSpecifier OpenValveSound = new SoundPathSpecifier("/Audio/Items/hiss.ogg");

    [DataField]
    public SoundSpecifier CloseValveSound = new SoundPathSpecifier("/Audio/Items/screwdriver.ogg");

    /// <summary>
    /// If the valve is open
    /// </summary>
    public bool ValveOpen = false;

    /// <summary>
    /// Time it takes to toggle the valve, gives atmos system time to process a potential gas tank rupture.
    /// </summary>
    public float Cooldown = 0.25f;

    /// <summary>
    /// When the valve can be opened again
    /// </summary>
    public TimeSpan NextToggle = TimeSpan.Zero;

    [DataField("togglePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string TogglePort = "Toggle";

    /// <summary>
    /// A debounce to guard against rapid signalling for the TTV to open before atmos can process the merge
    /// </summary>
    public bool ToggleDebounce = true;

    /// <summary>
    /// A flag for if the ttv changed the volume of one of the attached tanks such that the volume can be properly reset when split.
    /// </summary>
    public bool SelfVolumeChanged = false;

    #region: Visual state data

    /// <summary>
    /// The state of the valve when nothing is attached.
    /// </summary>
    [DataField]
    public string EmptyState = "valve_1";

    /// <summary>
    /// The state of the valve when any tanks or signals are connects.
    /// </summary>
    [DataField]
    public string AttachedState = "valve";

    /// <summary>
    /// The state to use as the device to show an active connection.
    [DataField]
    public string DeviceState = "device";

    /// <summary>
    /// The state to use as the right tank if none is provided.
    [DataField]
    public string DefaultRightState = "oxygen";

    /// <summary>
    /// The state to use as the left tank if none is provided.
    /// </summary>
    [DataField]
    public string DefaultLeftState = "plasma";

    /// <summary>
    /// The state on an inserted gas tank's Rsi to use to show the tank.
    /// </summary>
    [DataField]
    public string TankRsiState = "transfervalve";

    #endregion
}


[Serializable, NetSerializable]
public enum TransferValveVisuals : byte
{
    Valve,
    RightTank,
    LeftTank,
    Device
}
