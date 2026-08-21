using Content.Shared.Atmos.Visuals;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Power.EntitySystems;
using Content.Goobstation.Shared.Atmos.Visuals;

namespace Content.Goobstation.Shared.Atmos.Portable.Systems;

public abstract class SharedPortablePumpSystem : EntitySystem
{
    [Dependency] protected readonly ItemSlotsSystem Slots = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UI = default!;
    [Dependency] protected readonly SharedPowerReceiverSystem Power = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly ISharedAdminLogManager AdminLogger = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PortablePumpComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<PortablePumpComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<PortablePumpComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        SubscribeLocalEvent<PortablePumpComponent, PortablePumpToggleMessage>(OnToggle);
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
            var newState = new PortablePumpBoundUserInterfaceState(lastState.Pressure, lastState.Connected, null, -1f);
            UI.SetUiState(ent.Owner, PortablePumpUiKey.Key, newState);
        }

        DirtyUI(ent);
    }

    private void OnToggle(Entity<PortablePumpComponent> ent, ref PortablePumpToggleMessage args)
    {
        var powerState = Power.TogglePower(ent);
        AdminLogger.Add(LogType.AtmosPowerChanged, $"{ToPrettyString(args.Actor)} turned {(powerState ? "On" : "Off")} {ToPrettyString(ent)}");
        Appearance.SetData(ent, PortablePumpVisuals.IsRunning, Power.IsPowered(ent.Owner));
        DirtyUI(ent);
    }

    private void OnPumpDirectionToggleMessage(Entity<PortablePumpComponent> ent, ref PortablePumpTogglePumpDirectionMessage args)
    {
        //Funny magic number that makes this a little more compact
        ent.Comp.PumpDirection ^= (VentPumpDirection) 1;
        AdminLogger.Add(LogType.AtmosDeviceSetting, $"{ToPrettyString(args.Actor):player} set the target pressure on Portable pump {ToPrettyString(ent.Owner):device} to {(ent.Comp.PumpDirection == VentPumpDirection.Releasing ? "releasing" : "siphoning")}.");
        Dirty(ent);
        DirtyUI(ent);
    }

    private void OnSetPumpPressureMessage(Entity<PortablePumpComponent> ent, ref PortablePumpSetPumpPressureMessage args)
    {
        float pressure = Math.Clamp(args.Pressure, 0, ent.Comp.MaximumPressure);
        ent.Comp.TargetPressure = pressure;
        AdminLogger.Add(LogType.AtmosPressureChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the pressure on {ToPrettyString(ent):device} to {pressure}kPa");
        Dirty(ent);
        DirtyUI(ent);
    }

    protected abstract void DirtyUI(Entity<PortablePumpComponent> ent);
}
