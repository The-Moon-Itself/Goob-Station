
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Explosion;

/// <summary>
///     Raised as a broadcast whenever an explosion goes off. Broadcasted right before the explosion is made.
/// </summary>
[ByRefEvent]
public record struct GlobalExplosionEvent(MapCoordinates Epicenter, float Intensity, float OrigIntensity, float IntensitySlope, float MaxIntensity)
{
    /// <summary>
    /// The epicenter of the explosion
    /// </summary>
    public readonly MapCoordinates Epicenter = Epicenter;
    /// <summary>
    /// The total intensity of the explosion before any cap
    /// </summary>
    public readonly float Intensity = Intensity;

    /// <summary>
    /// The total intensity of the explosion after apply the maximum cap.
    /// </summary>
    public readonly float OrigIntensity = OrigIntensity;

    /// <summary>
    /// The intensity slope of the explosion
    /// </summary>
    public readonly float IntensitySlope = IntensitySlope;

    /// <summary>
    /// The maximum intensity of the explosion
    /// </summary>
    public readonly float MaxIntensity = MaxIntensity;
}
