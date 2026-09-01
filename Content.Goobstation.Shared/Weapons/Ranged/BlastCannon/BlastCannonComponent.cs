
using Content.Shared.Containers.ItemSlots;

namespace Content.Goobstation.Shared.Weapons.Ranged;


/// <summary>
///     Component for the blast cannon,
/// </summary>
[RegisterComponent]
public sealed partial class BlastCannonComponent : Component
{

    /// <summary>
    /// The slot for the tank transfer valve
    /// </summary>
    [DataField(required: true)]
    public ItemSlot TransferValveSlot = new();

    /// <summary>
    /// If true, the blast cannon won't make space tiles.
    /// </summary>
    [DataField]
    public bool HugBox = false;

    /// <summary>
    /// Fixes blastwaves fired by this blast cannon to this value if set.
    /// </summary>
    [DataField]
    public float? DebugPower = null;

    /// <summary>
    /// Scales knockback on a GunRecoil component by the blastwave's power if the blast cannon isn't wielded on firing.
    /// </summary>
    [DataField]
    public float RecoilMultiplier = 2f;
}
