using Content.Shared.Atmos.Visuals;
using Content.Shared.Atmos.Portable.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;

namespace Content.Shared.Atmos.Portable.Systems;

public abstract class SharedPortableScrubberSystem : EntitySystem
{
    [Dependency] protected readonly ItemSlotsSystem Slots = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UI = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PortableScrubberComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<PortableScrubberComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<PortableScrubberComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        SubscribeLocalEvent<PortableScrubberComponent, PortableScrubberEjectTankMessage>(OnHoldingTankEjectMessage);
    }

    private void OnComponentInit(Entity<PortableScrubberComponent> ent, ref ComponentInit args)
    {
        Slots.AddItemSlot(ent, ent.Comp.ContainerName, ent.Comp.GasTankSlot);
    }

    private void OnItemInserted(Entity<PortableScrubberComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerName)
            return;
        Appearance.SetData(ent, PortableScrubberVisuals.HasTank, true);

        DirtyUI(ent);
    }

    private void OnItemRemoved(Entity<PortableScrubberComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerName)
            return;
        Appearance.SetData(ent, PortableScrubberVisuals.HasTank, false);

        DirtyUI(ent);
    }

    private void OnHoldingTankEjectMessage(Entity<PortableScrubberComponent> ent, ref PortableScrubberEjectTankMessage args)
    {
        if (!ent.Comp.GasTankSlot.HasItem)
            return;
        Slots.TryEjectToHands(ent, ent.Comp.GasTankSlot, args.Actor, excludeUserAudio: true);

        if (UI.TryGetUiState<PortableScrubberBoundUserInterfaceState>(ent.Owner, PortableScrubberUiKey.Key, out var lastState))
        {
            var newState = new PortableScrubberBoundUserInterfaceState(lastState.Enabled, lastState.Pressure, lastState.IsFull, lastState.Connected, lastState.FilterGases, null, -1f);
            UI.SetUiState(ent.Owner, PortableScrubberUiKey.Key, newState);
        }

        DirtyUI(ent);
    }

    protected abstract void DirtyUI(Entity<PortableScrubberComponent> ent);
}
