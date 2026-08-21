using Robust.Client.GameObjects;
using Content.Goobstation.Shared.Atmos.Visuals;

namespace Content.Goobstation.Client.Atmos.Visualizers;

/// <summary>
/// Controls client-side visuals for portable pumps.
/// </summary>
public sealed class PortablePumpSystem : VisualizerSystem<PortablePumpVisualsComponent>
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void OnAppearanceChange(EntityUid uid, PortablePumpVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<bool>(uid, PortablePumpVisuals.IsFull, out var isFull, args.Component)
            && AppearanceSystem.TryGetData<bool>(uid, PortablePumpVisuals.IsRunning, out var isRunning, args.Component))
        {
            var runningState = isRunning ? component.RunningState : component.IdleState;
            _sprite.LayerSetRsiState((uid, args.Sprite), PortablePumpVisualLayers.IsRunning, runningState);

            _sprite.LayerSetVisible((uid, args.Sprite), PortablePumpVisualLayers.IsFull, isRunning && !isFull);
        }

        if (AppearanceSystem.TryGetData<bool>(uid, PortablePumpVisuals.IsConnected, out var isConnected, args.Component))
        {
            _sprite.LayerSetVisible((uid, args.Sprite), PortablePumpVisualLayers.IsConnected, isConnected);
        }

        if (AppearanceSystem.TryGetData<bool>(uid, PortablePumpVisuals.HasTank, out var hasTank, args.Component))
        {
            _sprite.LayerSetVisible((uid, args.Sprite), PortablePumpVisualLayers.HasTank, hasTank);
        }
    }
}

public enum PortablePumpVisualLayers : byte
{
    IsRunning,
    IsFull,
    IsConnected,
    HasTank
}
