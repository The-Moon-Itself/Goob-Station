using Robust.Client.GameObjects;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.TransferValve.Components;

namespace Content.Client.TransferValve;

public sealed class TransferValveSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TransferValveComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, TransferValveComponent component, ref AppearanceChangeEvent args)
    {
        var sprite = args.Sprite;
        var appearance = args.Component;
        if (sprite == null || appearance == null)
            return;
        var ttv = (uid, sprite);

        var hasRightTank = UpdateTankLayerState(uid, sprite, appearance, TransferValveVisuals.RightTank, component.TankRsiState, component.DefaultRightState);
        var hasLeftTank = UpdateTankLayerState(uid, sprite, appearance, TransferValveVisuals.LeftTank, component.TankRsiState, component.DefaultLeftState);

        _sprite.LayerSetRsiState(ttv, TransferValveVisuals.Valve,
                    (hasRightTank || hasLeftTank) ? component.AttachedState : component.EmptyState);
    }

    private bool UpdateTankLayerState(EntityUid uid, SpriteComponent sprite, AppearanceComponent appearance, Enum layerKey, string tankState, string defaultState)
    {
        var ttv = (uid, sprite);

        if (!_appearance.TryGetData<NetEntity>(uid, layerKey, out var tankNetEntity, appearance))
        {
            _sprite.LayerSetVisible(ttv, layerKey, false);
            return false;
        }
        _sprite.LayerSetVisible(ttv, layerKey, true);

        if (_entManager.TryGetEntity(tankNetEntity, out var tankId)
            && _entManager.TryGetComponent<SpriteComponent>(tankId, out var tankSprite))
        {
            var tankRsi = tankSprite.BaseRSI;
            if (tankRsi != null && tankRsi.TryGetState(tankState, out _))
            {
                _sprite.LayerSetRsi(ttv, layerKey, tankRsi, tankState);
                return true;
            }
        }
        _sprite.LayerSetRsi(ttv, layerKey, sprite.BaseRSI, defaultState);
        return true;
    }
}
