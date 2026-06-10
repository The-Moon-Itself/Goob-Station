using Content.Server.Atmos.EntitySystems;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Examine;
using Content.Shared.TransferValve.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server.TransferValve;

public sealed class TransferValveSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private const float ToggleDebounceDuration = 0.25f;
    private static readonly Dictionary<EntityUid, float> DebouncedTimes = new();
    private const string Tank1SlotId = "Tank1";
    private const string Tank2SlotId = "Tank2";


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TransferValveComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<TransferValveComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<TransferValveComponent, EntInsertedIntoContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<TransferValveComponent, EntRemovedFromContainerMessage>(OnItemSlotChanged);

        SubscribeLocalEvent<TransferValveComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<TransferValveComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        SubscribeLocalEvent<TransferValveComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnComponentInit(EntityUid uid, TransferValveComponent component, ComponentInit args)
    {
        if (_itemSlotsSystem.TryGetSlot(uid, Tank1SlotId, out var slot))
            component.Tank1Slot = slot;
        else
            _itemSlotsSystem.AddItemSlot(uid, Tank1SlotId, component.Tank1Slot);

        if (_itemSlotsSystem.TryGetSlot(uid, Tank2SlotId, out slot))
            component.Tank2Slot = slot;
        else
            _itemSlotsSystem.AddItemSlot(uid, Tank2SlotId, component.Tank2Slot);

        _signalSystem.EnsureSinkPorts(uid, component.TogglePort);
    }

    private void OnComponentRemove(EntityUid uid, TransferValveComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.Tank1Slot);
        _itemSlotsSystem.RemoveItemSlot(uid, component.Tank2Slot);
        DebouncedTimes.Remove(uid);
    }

    private void OnItemSlotChanged(EntityUid uid, TransferValveComponent component, ContainerModifiedMessage args)
    {
        if (!component.Initialized)
            return;

        bool hasAppearance = TryComp<AppearanceComponent>(uid, out var appearance);
        NetEntity? tankNet;
        if (args.Container.ID == component.Tank1Slot.ID)
        {
            _itemSlotsSystem.SetLock(uid, component.Tank1Slot, component.Tank1Slot.HasItem);
            if (hasAppearance)
            {
                if (component.Tank1Slot.HasItem
                    && _entManager.TryGetComponent<GasTankComponent>(component.Tank1Slot.Item, out var tank1Comp))
                {
                    tankNet = _entManager.GetNetEntity(component.Tank1Slot.Item);
                    if (tankNet.HasValue)
                    {
                        _appearance.SetData(uid, TransferValveVisuals.RightTank, tankNet.Value, appearance);
                        return;
                    }
                }
                _appearance.RemoveData(uid, TransferValveVisuals.RightTank, appearance);
            }
            return;
        }

        if (args.Container.ID == component.Tank2Slot.ID)
        {
            _itemSlotsSystem.SetLock(uid, component.Tank2Slot, component.Tank2Slot.HasItem);
            if (hasAppearance)
            {
                if (component.Tank2Slot.HasItem
                    && _entManager.TryGetComponent<GasTankComponent>(component.Tank2Slot.Item, out var tank2Comp))
                {
                    tankNet = _entManager.GetNetEntity(component.Tank2Slot.Item);
                    if (tankNet.HasValue)
                    {
                        _appearance.SetData(uid, TransferValveVisuals.LeftTank, tankNet, appearance);
                        return;
                    }
                }
                _appearance.RemoveData(uid, TransferValveVisuals.LeftTank, appearance);
            }
            return;
        }
    }

    private void OnExamined(EntityUid uid, TransferValveComponent component, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("transfer-valve-examine", ("state", component.ValveOpen ? "open" : "closed")));
    }

    private void OnGetVerbs(EntityUid uid, TransferValveComponent component, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract)
            return;

        if (component.Tank1Slot.HasItem)
        {
            AlternativeVerb detach1 = new()
            {
                Act = () => DetachTank(uid, component, component.Tank1Slot),
                Text = Loc.GetString("transfer-valve-detach-right-tank"),
                Priority = 1
            };
            args.Verbs.Add(detach1);
        }

        if (component.Tank2Slot.HasItem)
        {
            AlternativeVerb detach2 = new()
            {
                Act = () => DetachTank(uid, component, component.Tank2Slot),
                Text = Loc.GetString("transfer-valve-detach-left-tank"),
                Priority = 2
            };
            args.Verbs.Add(detach2);
        }

        if (component.Tank1Slot.HasItem && component.Tank2Slot.HasItem)
        {
            var toggleText = component.ValveOpen ? "Close" : "Open";
            AlternativeVerb toggle = new()
            {
                Act = () => ToggleValve(uid, component),
                Text = Loc.GetString("transfer-valve-toggle-verb", ("action", toggleText)),
                Priority = 3
            };
            args.Verbs.Add(toggle);
        }
    }

    private void DetachTank(EntityUid uid, TransferValveComponent component, ItemSlot tankSlot)
    {
        if (!tankSlot.HasItem)
            return;
        SplitGases(component);
        if (component.ValveOpen)
            _audioSystem.PlayPredicted(component.CloseValveSound, uid, null);
        component.ValveOpen = false;
        _itemSlotsSystem.SetLock(uid, tankSlot, false);
        _itemSlotsSystem.TryEject(uid, tankSlot, null, out _);
    }

    private void SplitGases(TransferValveComponent component)
    {
        if (!component.ValveOpen ||
            !_entManager.TryGetComponent<GasTankComponent>(component.Tank1Slot.Item, out GasTankComponent? tank_one) ||
            !_entManager.TryGetComponent<GasTankComponent>(component.Tank2Slot.Item, out GasTankComponent? tank_two))
            return;
        GasMixture mix_one = tank_one.Air;
        GasMixture mix_two = tank_two.Air;
        float volume_ratio = mix_one.Volume / mix_two.Volume;
        GasMixture temp = mix_two.RemoveRatio(volume_ratio);
        _atmosphereSystem.Merge(mix_one, temp);
        if (component.SelfVolumeChanged)
        {
            mix_two.Volume -= mix_one.Volume;
            component.SelfVolumeChanged = false;
        }
    }

    public bool MergeGases(TransferValveComponent component, GasMixture? target = null, bool change_volume = true)
    {
        if (!_entManager.TryGetComponent<GasTankComponent>(component.Tank1Slot.Item, out GasTankComponent? tank_one) ||
            !_entManager.TryGetComponent<GasTankComponent>(component.Tank2Slot.Item, out GasTankComponent? tank_two))
            return false;
        GasMixture mix_one = tank_one.Air;
        GasMixture mix_two = tank_two.Air;
        bool target_self = false;
        if (target == null || ReferenceEquals(target, mix_one))
            target = mix_two;
        if (ReferenceEquals(target, mix_two))
            target_self = true;

        if (change_volume)
        {
            if (!target_self)
                target.Volume += mix_two.Volume;
            else
                component.SelfVolumeChanged = true;
            target.Volume += mix_one.Volume;
        }
        _atmosphereSystem.Merge(target, mix_one.RemoveRatio(1));
        if (!target_self)
            _atmosphereSystem.Merge(target, mix_two.RemoveRatio(1));
        return true;
    }

    public void ToggleValve(EntityUid uid, TransferValveComponent component)
    {
        if (!_entManager.TryGetComponent<GasTankComponent>(component.Tank1Slot.Item, out GasTankComponent? tank_one) ||
            !_entManager.TryGetComponent<GasTankComponent>(component.Tank2Slot.Item, out GasTankComponent? tank_two))
            return;
        if (!component.ValveOpen)
        {
            component.ValveOpen = MergeGases(component);
            if (component.ValveOpen)
                _audioSystem.PlayPredicted(component.OpenValveSound, uid, null);
        }
        else
        {
            SplitGases(component);
            component.ValveOpen = false;
            _audioSystem.PlayPredicted(component.CloseValveSound, uid, null);
        }
    }

    public void OnSignalReceived(EntityUid uid, TransferValveComponent component, SignalReceivedEvent args)
    {
        if (args.Port == component.TogglePort && component.ToggleDebounce)
        {
            component.ToggleDebounce = false;
            ToggleValve(uid, component);
            DebouncedTimes[uid] = ToggleDebounceDuration;
            return;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (DebouncedTimes.Count == 0)
            return;

        var toRemove = new List<EntityUid>();

        foreach (var kvp in DebouncedTimes)
        {
            var uid = kvp.Key;
            var timeLeft = kvp.Value - frameTime;

            if (timeLeft <= 0f)
            {
                if (_entManager.TryGetComponent<TransferValveComponent>(uid, out var comp))
                {
                    comp.ToggleDebounce = true;
                }

                toRemove.Add(uid);
            }
            else
            {
                DebouncedTimes[uid] = timeLeft;
            }
        }

        foreach (var uid in toRemove)
        {
            DebouncedTimes.Remove(uid);
        }
    }
}
