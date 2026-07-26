

using Content.Goobstation.Shared.DopplerArray;

namespace Content.Goobstation.Server.DopplerArray;

[RegisterComponent]
public sealed partial class DopplerArrayComponent : Component
{
    /// <summary>
    /// How far the doppler array can detect explosions.
    /// </summary>
    public float MaxDistance = 150f;
    /// <summary>
    /// The number to be given to the name of the next record.
    /// </summary>
    public int RecordNumber = 1;

    public List<TachyonRecord> Records = new();
}
