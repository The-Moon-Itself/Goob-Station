
using Robust.Shared.Map;
using Robust.Shared.Serialization;

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
    [DataField]
    public string Name = "Log Recording";


    /// <summary>
    /// When the explosion occured
    /// </summary>
    [DataField]
    public TimeSpan Timestamp;


    /// <summary>
    /// The location of the epicenter of the explosion
    /// </summary>
    [DataField]
    public MapCoordinates Coordinates;


    /// <summary>
    /// The radius of the explosion after the intensity was capped
    /// </summary>
    [DataField]
    public float FactualRadius = 0;

    /// <summary>
    /// The radius of the explosion before the intensity was capped
    /// </summary>
    [DataField]
    public float TheoreticalRadius = 0;

    public TachyonRecord(string name, TimeSpan timestamp, MapCoordinates coordinates, float factualRadius, float theoreticalRadius)
    {
        Name = name;
        Timestamp = timestamp;
        Coordinates = coordinates;
        FactualRadius = factualRadius;
        TheoreticalRadius = theoreticalRadius;
    }
}
