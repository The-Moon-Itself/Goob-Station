
using Content.Goobstation.Shared.TransferValve.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Content.Shared.Weapons.Ranged.Events;
using Content.Goobstation.Shared.Weapons.Recoil;
using Content.Shared.Wieldable.Components;

namespace Content.Goobstation.Shared.Weapons.Ranged;

public abstract partial class SharedBlastCannonSystem : EntitySystem
{
    [Dependency] protected readonly MetaDataSystem MetaDataSystem = default!;
    [Dependency] protected readonly ItemSlotsSystem ItemSlotsSystem = default!;
    [Dependency] protected readonly IEntityManager EntManager = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    protected const string TTVSlotId = "TransferValveSlot";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlastCannonComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BlastCannonComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<BlastCannonComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }
    private void OnComponentInit(Entity<BlastCannonComponent> ent, ref ComponentInit args)
    {
        if (ItemSlotsSystem.TryGetSlot(ent.Owner, TTVSlotId, out var slot))
            ent.Comp.TransferValveSlot = slot;
        else
            ItemSlotsSystem.AddItemSlot(ent.Owner, TTVSlotId, ent.Comp.TransferValveSlot);

        UpdateAppearance(ent);
    }

    protected void OnEntInserted(Entity<BlastCannonComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!ent.Comp.Initialized || args.Container.ID != TTVSlotId)
            return;
        UpdateAppearance(ent);
    }

    private void OnEntRemoved(Entity<BlastCannonComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!ent.Comp.Initialized || args.Container.ID != TTVSlotId)
            return;
        UpdateAppearance(ent);
    }

    protected virtual void UpdateAppearance(Entity<BlastCannonComponent> ent)
    {
        var loaded = ent.Comp.TransferValveSlot.HasItem;
        MetaDataSystem.SetEntityName(ent, Loc.GetString(loaded ? "blast-cannon-loaded-name" : "blast-cannon-unloaded-name"));
        MetaDataSystem.SetEntityDescription(ent, Loc.GetString(loaded ? "blast-cannon-loaded-desc" : "blast-cannon-unloaded-desc"));
    }
}
