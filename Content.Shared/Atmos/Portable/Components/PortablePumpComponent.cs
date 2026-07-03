
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Atmos.Piping.Unary.Components;

namespace Content.Shared.Atmos.Portable
{
    [RegisterComponent]

    public sealed partial class PortablePumpComponent : Component
    {
        /// <summary>
        /// The air inside this machine.
        /// </summary>
        [DataField("gasMixture"), ViewVariables(VVAccess.ReadWrite)]
        public GasMixture Air { get; private set; } = new();

        [DataField("port"), ViewVariables(VVAccess.ReadWrite)]
        public string PortName { get; set; } = "port";

        public VentPumpDirection PumpDirection { get; set; } = VentPumpDirection.Releasing;
        public float TargetPressure { get; set; } = Atmospherics.OneAtmosphere;

        public float MaximumPressure { get; set; } = 25f * Atmospherics.OneAtmosphere;
        public float MinimumPressure { get; set; } = 0.1f * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Container name for the gas tank holder.
        /// </summary>
        [DataField("container")]
        public string ContainerName { get; set; } = "tank_slot";

        [DataField]
        public ItemSlot GasTankSlot = new();
    }
}
