

using Content.Goobstation.Shared.DopplerArray;

namespace Content.Goobstation.Server.DopplerArray;

[RegisterComponent]
public sealed partial class DopplerArrayComponent : Component
{
    /// <summary>
    /// How far the doppler array can detect explosions.
    /// </summary>
    [DataField]
    public float MaxDistance = 150f;

    /// <summary>
    /// How many seconds after sensing an explosion until the array is ready to sense the next explosion.
    /// </summary>
    [DataField]
    public float Cooldown = 5f;

    /// <summary>
    /// Timestamp for when the array is off cooldown.
    /// </summary>
    public TimeSpan NextAnnounce = TimeSpan.Zero;

    /// <summary>
    /// The number to be given to the name of the next record.
    /// </summary>
    public int RecordNumber = 1;

    public List<TachyonRecord> Records = new();
}
