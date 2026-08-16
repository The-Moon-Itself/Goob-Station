
using Content.Goobstation.Shared.TransferValve.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared.Weapons.Ranged;

public abstract partial class SharedBlastCannonSystem : EntitySystem
{
    [Dependency] protected readonly MetaDataSystem MetaData = default!;
    [Dependency] protected readonly ItemSlotsSystem ItemSlotsSystem = default!;
    [Dependency] protected readonly IEntityManager EntManager = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    protected const string TTVSlotId = "TransferValveSlot";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlastCannonComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BlastCannonComponent, AttemptShootEvent>(OnShootAttempt);
        SubscribeLocalEvent<BlastCannonComponent, AmmoShotEvent>(OnFired);
        SubscribeLocalEvent<BlastCannonComponent, ContainerIsInsertingAttemptEvent>(OnItemAddedAttempt);
        SubscribeLocalEvent<BlastCannonComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<BlastCannonComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }
    protected void OnComponentInit(Entity<BlastCannonComponent> ent, ref ComponentInit args)
    {
        if (ItemSlotsSystem.TryGetSlot(ent.Owner, TTVSlotId, out var slot))
            ent.Comp.TransferValveSlot = slot;
        else
            ItemSlotsSystem.AddItemSlot(ent.Owner, TTVSlotId, ent.Comp.TransferValveSlot);

        UpdateAppearance(ent);
    }

    protected virtual void OnShootAttempt(Entity<BlastCannonComponent> ent, ref AttemptShootEvent args)
    {
        return;
    }

    protected virtual void OnFired(Entity<BlastCannonComponent> ent, ref AmmoShotEvent args)
    {
        return;
    }

    protected virtual void OnItemAddedAttempt(Entity<BlastCannonComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        return;
    }

    protected void OnEntInserted(Entity<BlastCannonComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!ent.Comp.Initialized || args.Container.ID != TTVSlotId)
            return;
        UpdateAppearance(ent);
    }

    protected void OnEntRemoved(Entity<BlastCannonComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!ent.Comp.Initialized || args.Container.ID != TTVSlotId)
            return;
        UpdateAppearance(ent);
    }

    protected virtual void UpdateAppearance(Entity<BlastCannonComponent> ent)
    {
        var loaded = ent.Comp.TransferValveSlot.HasItem;
        MetaData.SetEntityName(ent, Loc.GetString(loaded ? "blast-cannon-loaded-name" : "blast-cannon-unloaded-name"));
        MetaData.SetEntityDescription(ent, Loc.GetString(loaded ? "blast-cannon-loaded-desc" : "blast-cannon-unloaded-desc"));
    }
}
