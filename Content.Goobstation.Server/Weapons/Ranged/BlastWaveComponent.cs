
using System.Numerics;
using Content.Shared.Explosion;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Weapons.Ranged;

/// <summary>
/// Causes explosive damage throughout the trajectory of the projectile
/// </summary>
[RegisterComponent]
public sealed partial class BlastWaveComponent : Component
{
    /// <summary>
    /// Distance the blastwave should travel before starting to explode tiles. Prevents exploding the firer if they are standing between tiles.
    /// Setting this to zero can cause the blast wave to prematurely delete itself if Power is also set to 0 initially.
    /// </summary>
    [DataField]
    public float Startup = 0.5f;

    /// <summary>
    /// The power of the blast wave, the distance left it can travel.
    /// </summary>
    [DataField]
    public float Power = 0;

    /// <summary>
    /// Caps the actual intensity that tiles and entities will feel
    /// </summary>
    [DataField]
    public float MaxIntensity = 4;

    /// <summary>
    /// How quickly the blastwave loses intensity.
    /// </summary>
    [DataField]
    public float IntensitySlope = 1;

    /// <summary>
    /// If true, the blastwave won't make space tiles.
    /// </summary>
    [DataField]
    public bool HugBox = false;

    /// <summary>
    ///     Factor used to scale the explosion intensity when calculating tile break chances. Allows for stronger
    ///     explosives that don't space tiles, without having to create a new explosion-type prototype.
    /// </summary>
    [DataField]
    public float TileBreakScale = 1f;

    /// <summary>
    ///     Maximum number of times that an explosive can break a tile. Currently, for normal space stations breaking a
    ///     tile twice will generally result in a vacuum.
    /// </summary>
    [DataField]
    public int MaxTileBreak = int.MaxValue;

    /// <summary>
    ///     Converts grid coordinates to space coordinates
    /// </summary>
    public Matrix3x2 SpaceMatrix = Matrix3x2.Identity;
    /// <summary>
    ///     The inverse of the space matrix
    /// </summary>
    public Matrix3x2 InvSpaceMatrix = Matrix3x2.Identity;

    /// <summary>
    /// Who to blame for the blastwave
    /// </summary>
    public EntityUid? Cause = null;

    /// <summary>
    /// Where the blast wave started from. A stand in for epicenter for explosion calculations
    /// </summary>
    public MapCoordinates Origin;

    /// <summary>
    ///     The ProtoID of ExplosionProto.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ExplosionPrototype> ExplosionType = default!;

    /// <summary>
    ///     The explosion prototype. This determines the damage types, the tile-break chance, and some visual
    ///     information (e.g., the light that the explosion gives off).
    /// </summary>
    public ExplosionPrototype? ExplosionProto;

    /// <summary>
    /// Entities that have been processed by this blastwave, so we don't damage something multiple times.
    /// </summary>
    public readonly HashSet<EntityUid> ProcessedEntities = new();

    /// <summary>
    /// Tiles that have already been processed by this blastwave.
    /// </summary>
    public readonly Dictionary<Entity<MapGridComponent>, List<(Vector2i, Tile)>> ProcessedTiles = new();

    public bool JustTeleported = false;

}
