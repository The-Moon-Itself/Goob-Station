
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Atmos.Portable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]

public sealed partial class PortablePumpComponent : Component
{
    /// <summary>
    /// The air inside this machine.
    /// </summary>
    [DataField("gasMixture"), ViewVariables(VVAccess.ReadWrite)]
    public GasMixture Air { get; private set; } = new();

    [DataField("port"), ViewVariables(VVAccess.ReadWrite)]
    public string PortName { get; set; } = "port";

    [DataField, AutoNetworkedField]
    public VentPumpDirection PumpDirection { get; set; } = VentPumpDirection.Releasing;
    [DataField, AutoNetworkedField]
    public float TargetPressure { get; set; } = Atmospherics.OneAtmosphere;

    [DataField, AutoNetworkedField]
    public float MaximumPressure { get; set; } = 25f * Atmospherics.OneAtmosphere;

    /// <summary>
    ///     Container name for the gas tank holder.
    /// </summary>
    [DataField("container")]
    public string ContainerName { get; set; } = "tank_slot";

    [DataField]
    public ItemSlot GasTankSlot = new();
}
