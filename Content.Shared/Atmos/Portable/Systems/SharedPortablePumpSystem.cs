using Content.Shared.Atmos.Visuals;
using Content.Shared.Atmos.Portable.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Content.Shared.Atmos.Piping.Unary.Components;

namespace Content.Shared.Atmos.Portable.Systems;

public abstract class SharedPortablePumpSystem : EntitySystem
{
    [Dependency] protected readonly ItemSlotsSystem Slots = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UI = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PortablePumpComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<PortablePumpComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<PortablePumpComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        SubscribeLocalEvent<PortablePumpComponent, PortablePumpEjectTankMessage>(OnHoldingTankEjectMessage);
        SubscribeLocalEvent<PortablePumpComponent, PortablePumpTogglePumpDirectionMessage>(OnPumpDirectionToggleMessage);
        SubscribeLocalEvent<PortablePumpComponent, PortablePumpSetPumpPressureMessage>(OnSetPumpPressureMessage);
    }

    private void OnComponentInit(Entity<PortablePumpComponent> ent, ref ComponentInit args)
    {
        Slots.AddItemSlot(ent, ent.Comp.ContainerName, ent.Comp.GasTankSlot);
    }

    private void OnItemInserted(Entity<PortablePumpComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerName)
            return;
        Appearance.SetData(ent, PortablePumpVisuals.HasTank, true);

        DirtyUI(ent);
    }

    private void OnItemRemoved(Entity<PortablePumpComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerName)
            return;
        Appearance.SetData(ent, PortablePumpVisuals.HasTank, false);

        DirtyUI(ent);
    }

    private void OnHoldingTankEjectMessage(Entity<PortablePumpComponent> ent, ref PortablePumpEjectTankMessage args)
    {
        if (!ent.Comp.GasTankSlot.HasItem)
            return;
        Slots.TryEjectToHands(ent, ent.Comp.GasTankSlot, args.Actor, excludeUserAudio: true);

        if (UI.TryGetUiState<PortablePumpBoundUserInterfaceState>(ent.Owner, PortablePumpUiKey.Key, out var lastState))
        {
            var newState = new PortablePumpBoundUserInterfaceState(lastState.Enabled, lastState.Pressure, lastState.Connected, lastState.PumpDirection, lastState.TargetPressure, null, -1f);
            UI.SetUiState(ent.Owner, PortableScrubberUiKey.Key, newState);
        }

        DirtyUI(ent);
    }

    private void OnPumpDirectionToggleMessage(Entity<PortablePumpComponent> ent, ref PortablePumpTogglePumpDirectionMessage args)
    {
        //Funny magic number that makes this a little more compact
        ent.Comp.PumpDirection ^= (VentPumpDirection) 1;
        if (UI.TryGetUiState<PortablePumpBoundUserInterfaceState>(ent.Owner, PortablePumpUiKey.Key, out var lastState))
        {
            var newState = new PortablePumpBoundUserInterfaceState(lastState.Enabled, lastState.Pressure, lastState.Connected, ent.Comp.PumpDirection, lastState.TargetPressure, lastState.TankLabel, lastState.TankPressure);
            UI.SetUiState(ent.Owner, PortableScrubberUiKey.Key, newState);
        }

        DirtyUI(ent);
    }

    private void OnSetPumpPressureMessage(Entity<PortablePumpComponent> ent, ref PortablePumpSetPumpPressureMessage args)
    {
        float pressure = Math.Clamp(args.Pressure, ent.Comp.MinimumPressure, ent.Comp.MaximumPressure);
        ent.Comp.TargetPressure = pressure;
        if (UI.TryGetUiState<PortablePumpBoundUserInterfaceState>(ent.Owner, PortablePumpUiKey.Key, out var lastState))
        {
            var newState = new PortablePumpBoundUserInterfaceState(lastState.Enabled, lastState.Pressure, lastState.Connected, lastState.PumpDirection, pressure, lastState.TankLabel, lastState.TankPressure);
            UI.SetUiState(ent.Owner, PortableScrubberUiKey.Key, newState);
        }

        DirtyUI(ent);
    }

    protected abstract void DirtyUI(Entity<PortablePumpComponent> ent);
}
