// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Guidebook;
using Robust.Shared.GameStates;

//Converted to shared and moved to goob
namespace Content.Goobstation.Shared.Atmos.Portable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class PortableScrubberComponent : Component
{
    /// <summary>
    /// The air inside this machine.
    /// </summary>
    [DataField("gasMixture"), ViewVariables(VVAccess.ReadWrite)]
    public GasMixture Air { get; private set; } = new();

    [DataField("port"), ViewVariables(VVAccess.ReadWrite)]
    public string PortName { get; set; } = "port";

    /// <summary>
    /// Which gases this machine will scrub out.
    /// Unlike fixed scrubbers controlled by an air alarm,
    /// this can't be changed in game.
    /// </summary>
    [DataField("filterGases"), AutoNetworkedField]
    public HashSet<Gas> FilterGases = new()
    {
        Gas.CarbonDioxide,
        Gas.Plasma,
        Gas.Tritium,
        Gas.WaterVapor,
        Gas.Ammonia,
        Gas.NitrousOxide,
        Gas.Frezon,
        Gas.BZ, // Assmos - /tg/ gases
        Gas.Healium, // Assmos - /tg/ gases
        Gas.Nitrium, // Assmos - /tg/ gases
    };

    /// <summary>
    /// Maximum internal pressure before it refuses to take more.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaxPressure = 2500;

    /// <summary>
    /// The speed at which gas is scrubbed from the environment.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float TransferRate = 800;

    /// <summary>
    ///     Container name for the gas tank holder.
    /// </summary>
    [DataField("container")]
    public string ContainerName { get; set; } = "tank_slot";

    [DataField]
    public ItemSlot GasTankSlot = new();

    #region GuidebookData

    [GuidebookData]
    public float Volume => Air.Volume;

    #endregion
}
