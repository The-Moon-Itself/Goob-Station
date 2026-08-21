using Content.Shared.Atmos.Visuals;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;

namespace Content.Goobstation.Shared.Atmos.Portable;

public abstract class SharedPortableScrubberSystem : EntitySystem
{
    [Dependency] protected readonly ItemSlotsSystem Slots = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UI = default!;
    [Dependency] protected readonly SharedPowerReceiverSystem Power = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly ISharedAdminLogManager AdminLogger = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PortableScrubberComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<PortableScrubberComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<PortableScrubberComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        SubscribeLocalEvent<PortableScrubberComponent, PortableScrubberToggleMessage>(OnToggle);
        SubscribeLocalEvent<PortableScrubberComponent, PortableScrubberEjectTankMessage>(OnHoldingTankEjectMessage);
        SubscribeLocalEvent<PortableScrubberComponent, PortableScrubberFilterGasToggleMessage>(OnFilterGasToggled);
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
            var newState = new PortableScrubberBoundUserInterfaceState(lastState.Pressure, lastState.IsFull, lastState.Connected, null, -1f);
            UI.SetUiState(ent.Owner, PortableScrubberUiKey.Key, newState);
        }

        DirtyUI(ent);
    }

    protected virtual void OnToggle(Entity<PortableScrubberComponent> ent, ref PortableScrubberToggleMessage args)
    {
        var powerState = Power.TogglePower(ent, user: args.Actor);
        AdminLogger.Add(LogType.AtmosPowerChanged, $"{ToPrettyString(args.Actor)} turned {(powerState ? "On" : "Off")} {ToPrettyString(ent)}");
        DirtyUI(ent);
    }

    private void OnFilterGasToggled(Entity<PortableScrubberComponent> ent, ref PortableScrubberFilterGasToggleMessage args)
    {
        var added = ent.Comp.FilterGases.Add(args.ToggledGas);
        if (!added)
            ent.Comp.FilterGases.Remove(args.ToggledGas);
        AdminLogger.Add(LogType.AtmosFilterChanged, LogImpact.Medium,
                        $"{ToPrettyString(args.Actor):player} {(added ? "enabled" : "disabled")} filtering {args.ToggledGas.ToString()} on {ToPrettyString(ent):device}");
        Dirty(ent);
        DirtyUI(ent);
    }

    protected abstract void DirtyUI(Entity<PortableScrubberComponent> ent);
}
