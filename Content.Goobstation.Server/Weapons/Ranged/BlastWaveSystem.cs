
using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.BlockTeleport;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Explosion;
using Npgsql.Replication.PgOutput.Messages;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Weapons.Ranged;

public sealed class BlastWaveSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlastWaveComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlastWaveComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<BlastWaveComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<BlastWaveComponent, TeleportAttemptEvent>(OnTeleport);
    }

    private void OnStartup(Entity<BlastWaveComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Origin = _transform.ToMapCoordinates(Transform(ent).Coordinates);
        if (!_prototypeManager.TryIndex<ExplosionPrototype>(ent.Comp.ExplosionType, out var type))
            return;
        ent.Comp.ExplosionProto = type;
        UpdateSpaceMatrix(ent);
    }

    private void OnRemove(Entity<BlastWaveComponent> ent, ref ComponentRemove args)
    {
        SetTiles(ent);
    }

    private void OnTeleport(Entity<BlastWaveComponent> ent, ref TeleportAttemptEvent args)
    {
        ent.Comp.JustTeleported = true;
        ent.Comp.Origin = _transform.ToMapCoordinates(Transform(ent).Coordinates);
        SetTiles(ent);
        UpdateSpaceMatrix(ent);
    }
    private void OnMove(Entity<BlastWaveComponent> ent, ref MoveEvent args)
    {
        var from = _transform.GetMoverCoordinates(args.OldPosition);
        var to = _transform.GetMoverCoordinates(args.NewPosition);
        var dir = Vector2.Normalize(to.Position - from.Position);
        var fromMap = _transform.ToMapCoordinates(from);
        var toMap = _transform.ToMapCoordinates(to);

        // Skip teleports
        if (ent.Comp.JustTeleported)
        {
            ent.Comp.JustTeleported = false;
            return;
        }
        // Likely a teleport the event didn't catch.
        if (fromMap.MapId != toMap.MapId)
        {
            ent.Comp.Origin = toMap;
            SetTiles(ent);
            UpdateSpaceMatrix(ent);
            return;
        }

        var dist = (toMap.Position - fromMap.Position).Length();
        if (ent.Comp.Startup > 0)
        {
            ent.Comp.Startup = MathF.Max(ent.Comp.Startup - dist, 0);
            return;
        }

        if (ent.Comp.ExplosionProto == null || ent.Comp.Power <= 0)
        {
            QueueDel(ent);
            return;
        }


        if (_entManager.TryGetComponent<MapGridComponent>(from.EntityId, out var fromGrid) && _entManager.TryGetComponent<BroadphaseComponent>(from.EntityId, out var fromBroadphase))
        {
            var fromGridEnt = (from.EntityId, fromGrid);
            var fromGridIterator = new GridLineEnumerator(_map.CoordinatesToTile(from.EntityId, fromGrid, from), _map.CoordinatesToTile(from.EntityId, fromGrid, to));
            if (!ent.Comp.ProcessedTiles.TryGetValue(fromGridEnt, out var fromGridTiles))
            {
                fromGridTiles = new();
                ent.Comp.ProcessedTiles[fromGridEnt] = fromGridTiles;
            }
            fromGridIterator.MoveNext(); //Prevents iterating the tile we iterated last on the previous move event.
            while (fromGridIterator.MoveNext())
            {
                var curTile = _map.GetTileRef(fromGridEnt, fromGridIterator.Current);
                if (!curTile.Tile.IsEmpty)
                {
                    var intensity = GetIntensity(ent, fromMap, dir, _map.GridTileToWorld(from.EntityId, fromGrid, fromGridIterator.Current));
                    var canDamageFloor = _explosion.ExplodeTile(
                        fromBroadphase,
                        fromGridEnt,
                        fromGridIterator.Current,
                        0,
                        ent.Comp.ExplosionProto.DamagePerIntensity * intensity,
                        ent.Comp.Origin,
                        ent.Comp.ProcessedEntities,
                        ent.Comp.ExplosionProto.ID,
                        ent.Comp.ExplosionProto.FireStacks,
                        ent.Comp.ExplosionProto.Temperature,
                        intensity,
                        ent.Comp.Cause);
                    if (canDamageFloor)
                        _explosion.DamageFloorTile(curTile, intensity * ent.Comp.TileBreakScale, ent.Comp.MaxTileBreak, !ent.Comp.HugBox, fromGridTiles, ent.Comp.ExplosionProto);
                }
                else
                {
                    var intensity = GetIntensity(ent, fromMap, dir, _map.GridTileToWorld(from.EntityId, fromGrid, fromGridIterator.Current));
                    _explosion.ExplodeSpace(
                        fromBroadphase,
                        ent.Comp.SpaceMatrix,
                        ent.Comp.InvSpaceMatrix,
                        fromGridIterator.Current,
                        0,
                        ent.Comp.ExplosionProto.DamagePerIntensity * intensity,
                        ent.Comp.Origin,
                        ent.Comp.ProcessedEntities,
                        ent.Comp.ExplosionProto.ID,
                        ent.Comp.ExplosionProto.FireStacks,
                        ent.Comp.Cause
                        );
                }
            }
        }

        // Jumped to a new grid, explode that now
        if (
            _entManager.TryGetComponent<MapGridComponent>(to.EntityId, out var toGrid)
            && _entManager.TryGetComponent<BroadphaseComponent>(to.EntityId, out var toBroadphase)
            && toGrid != fromGrid)
        {
            //New grid, potential new space matrix
            UpdateSpaceMatrix(ent);
            var toGridEnt = (to.EntityId, toGrid);
            var toGridIterator = new GridLineEnumerator(_map.CoordinatesToTile(to.EntityId, toGrid, from), _map.CoordinatesToTile(to.EntityId, toGrid, to));
            if (!ent.Comp.ProcessedTiles.TryGetValue(toGridEnt, out var toGridTiles))
            {
                toGridTiles = new();
                ent.Comp.ProcessedTiles[toGridEnt] = toGridTiles;
            }
            // flag to prevent iterating space tiles that the from iteration should have processed already
            bool enteredGridFlag = false;
            //Don't need to preemptively iterate once here because it's a different grid
            while (toGridIterator.MoveNext())
            {
                var curTile = _map.GetTileRef(toGridEnt, toGridIterator.Current);
                if (!curTile.Tile.IsEmpty)
                {
                    enteredGridFlag = true;
                    var intensity = GetIntensity(ent, fromMap, dir, _map.GridTileToWorld(to.EntityId, toGrid, toGridIterator.Current));
                    var canDamageFloor = _explosion.ExplodeTile(
                        toBroadphase,
                        toGridEnt,
                        toGridIterator.Current,
                        0,
                        ent.Comp.ExplosionProto.DamagePerIntensity * intensity,
                        ent.Comp.Origin,
                        ent.Comp.ProcessedEntities,
                        ent.Comp.ExplosionProto.ID,
                        ent.Comp.ExplosionProto.FireStacks,
                        ent.Comp.ExplosionProto.Temperature,
                        intensity,
                        ent.Comp.Cause);
                    if (canDamageFloor)
                        _explosion.DamageFloorTile(curTile, intensity * ent.Comp.TileBreakScale, ent.Comp.MaxTileBreak, !ent.Comp.HugBox, toGridTiles, ent.Comp.ExplosionProto);
                }
                else
                {
                    if (!enteredGridFlag)
                        continue;
                    var intensity = GetIntensity(ent, fromMap, dir, _map.GridTileToWorld(to.EntityId, toGrid, toGridIterator.Current));
                    _explosion.ExplodeSpace(
                        toBroadphase,
                        ent.Comp.SpaceMatrix,
                        ent.Comp.InvSpaceMatrix,
                        toGridIterator.Current,
                        0,
                        ent.Comp.ExplosionProto.DamagePerIntensity * intensity,
                        ent.Comp.Origin,
                        ent.Comp.ProcessedEntities,
                        ent.Comp.ExplosionProto.ID,
                        ent.Comp.ExplosionProto.FireStacks,
                        ent.Comp.Cause
                        );
                }
            }
        }

        ent.Comp.Power = MathF.Max(ent.Comp.Power - dist, 0);
        if (ent.Comp.Power <= 0)
        {
            QueueDel(ent);
        }

        if (ent.Comp.ProcessedTiles.Values.Sum(tiles => tiles.Count) > 7)
        {
            SetTiles(ent);
        }
    }

    private float GetIntensity(Entity<BlastWaveComponent> ent, MapCoordinates start, Vector2 dir, MapCoordinates sample)
    {
        if (ent.Comp.ExplosionProto == null)
            return 0;
        var offset = (sample.Position - start.Position);
        var dist = Vector2.Dot(offset, dir);
        // Because we're in a line and defined by range, not intensity, it makes the math a bit easier here.
        return MathF.Min((ent.Comp.Power - dist) * ent.Comp.IntensitySlope, ent.Comp.MaxIntensity);
    }


    private void UpdateSpaceMatrix(Entity<BlastWaveComponent> ent)
    {
        var (_, referenceGrid, _) = _explosion.GetLocalGrids(_transform.ToMapCoordinates(Transform(ent).Coordinates), ent.Comp.Power * ent.Comp.IntensitySlope, ent.Comp.IntensitySlope, ent.Comp.MaxIntensity);

        var spaceMatrix = Matrix3x2.Identity;
        if (referenceGrid != null)
        {
            var xform = Transform(referenceGrid.Value);
            (_, _, spaceMatrix) = _transform.GetWorldPositionRotationMatrix(xform);
        }
        ent.Comp.SpaceMatrix = spaceMatrix;
        Matrix3x2.Invert(spaceMatrix, out ent.Comp.InvSpaceMatrix);
    }

    private void SetTiles(Entity<BlastWaveComponent> ent)
    {
        foreach (var (grid, list) in ent.Comp.ProcessedTiles)
        {
            if (list.Count > 0 && _entManager.EntityExists(grid.Owner))
            {
                _map.SetTiles(grid.Owner, grid.Comp, list);
            }
        }
        ent.Comp.ProcessedTiles.Clear();
        ent.Comp.ProcessedEntities.Clear();
    }
}
