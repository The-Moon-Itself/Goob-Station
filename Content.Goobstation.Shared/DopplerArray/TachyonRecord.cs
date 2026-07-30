
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.DopplerArray;

/// <summary>
/// A record of an explosion detected by a doppler array.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public sealed partial record TachyonRecord
{
    /// <summary>
    /// The name of the log recording
    /// </summary>
    public string Name = "Log Recording";


    /// <summary>
    /// When the explosion occured
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan Timestamp;


    /// <summary>
    /// The location of the epicenter of the explosion
    /// </summary>
    public MapCoordinates Coordinates;


    /// <summary>
    /// The radius of the explosion after the intensity was capped
    /// </summary>
    public float FactualRadius = 0;

    /// <summary>
    /// The radius of the explosion before the intensity was capped
    /// </summary>
    public float TheoreticalRadius = 0;
}
