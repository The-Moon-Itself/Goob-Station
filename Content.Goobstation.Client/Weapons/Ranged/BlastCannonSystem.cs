
using Content.Goobstation.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Weapons.Ranged;

public sealed class BlastCannonSystem : SharedBlastCannonSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void UpdateAppearance(Entity<BlastCannonComponent> ent)
    {
        base.UpdateAppearance(ent);

        if (!EntManager.TryGetComponent<SpriteComponent>(ent, out var sprite))
            return;

        _sprite.LayerSetVisible((ent, sprite), BlastCannonVisualsLayers.ttv, ent.Comp.TransferValveSlot.HasItem);
    }
}

public enum BlastCannonVisualsLayers : byte
{
    icon,
    ttv
}
