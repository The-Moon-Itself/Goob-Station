using Content.Goobstation.Server.Weapons.Ranged;
using Content.Goobstation.Shared.TransferValve.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Database;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.TransferValve;

public sealed class TransferValveSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    private const string Tank1SlotId = "Tank1";
    private const string Tank2SlotId = "Tank2";


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TransferValveComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<TransferValveComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<TransferValveComponent, EntInsertedIntoContainerMessage>(OnItemAdded);
        SubscribeLocalEvent<TransferValveComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        SubscribeLocalEvent<TransferValveComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<TransferValveComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternateVerbs);
        SubscribeLocalEvent<TransferValveComponent, GetVerbsEvent<ActivationVerb>>(OnGetActivationVerbs);

        SubscribeLocalEvent<TransferValveComponent, SignalReceivedEvent>(OnSignalReceived);

        SubscribeLocalEvent<TransferValveComponent, AmmoShotEvent>(OnFired);
    }

    private void OnComponentInit(Entity<TransferValveComponent> ent, ref ComponentInit args)
    {
        if (_itemSlotsSystem.TryGetSlot(ent.Owner, Tank1SlotId, out var slot))
            ent.Comp.Tank1Slot = slot;
        else
            _itemSlotsSystem.AddItemSlot(ent.Owner, Tank1SlotId, ent.Comp.Tank1Slot);

        if (_itemSlotsSystem.TryGetSlot(ent.Owner, Tank2SlotId, out slot))
            ent.Comp.Tank2Slot = slot;
        else
            _itemSlotsSystem.AddItemSlot(ent.Owner, Tank2SlotId, ent.Comp.Tank2Slot);

        _signalSystem.EnsureSinkPorts(ent.Owner, ent.Comp.TogglePort);
    }

    private void OnComponentRemove(Entity<TransferValveComponent> ent, ref ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(ent, ent.Comp.Tank1Slot);
        _itemSlotsSystem.RemoveItemSlot(ent, ent.Comp.Tank2Slot);
    }

    private void OnItemAdded(Entity<TransferValveComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!ent.Comp.Initialized)
            return;

        TryComp<AppearanceComponent>(ent, out var appearance);
        if (args.Container.ID == ent.Comp.Tank1Slot.ID)
            UpdateSlot(ent, ent.Comp.Tank1Slot, appearance, TransferValveVisuals.RightTank);
        else if (args.Container.ID == ent.Comp.Tank2Slot.ID)
            UpdateSlot(ent, ent.Comp.Tank2Slot, appearance, TransferValveVisuals.LeftTank);
    }

    private void OnItemRemoved(Entity<TransferValveComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!ent.Comp.Initialized)
            return;

        TryComp<AppearanceComponent>(ent, out var appearance);
        if (args.Container.ID == ent.Comp.Tank1Slot.ID)
            UpdateSlot(ent, ent.Comp.Tank1Slot, appearance, TransferValveVisuals.RightTank);
        else if (args.Container.ID == ent.Comp.Tank2Slot.ID)
            UpdateSlot(ent, ent.Comp.Tank2Slot, appearance, TransferValveVisuals.LeftTank);
    }

    private void UpdateSlot(Entity<TransferValveComponent> ent, ItemSlot slot, AppearanceComponent? appearance, TransferValveVisuals tankLayer)
    {
        _itemSlotsSystem.SetLock(ent, slot, slot.HasItem);
        if (ent.Comp.Tank1Slot.HasItem && ent.Comp.Tank1Slot.HasItem)
            _adminLogger.Add(LogType.Explosion, LogImpact.Medium, $"{ToPrettyString(ent):entity} has been fully assembled and ready to detonante.");
        if (appearance != null)
        {
            if (slot.HasItem
                && _entManager.TryGetComponent<GasTankComponent>(slot.Item, out var tank1Comp))
            {
                var tankNet = _entManager.GetNetEntity(slot.Item);
                if (tankNet.HasValue)
                {
                    _appearance.SetData(ent, tankLayer, tankNet.Value, appearance);
                    return;
                }
            }
            _appearance.RemoveData(ent, tankLayer, appearance);
        }
    }

    private void OnExamined(Entity<TransferValveComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("transfer-valve-examine", ("state", ent.Comp.ValveOpen ? "open" : "closed")));
    }

    private void OnGetAlternateVerbs(Entity<TransferValveComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract)
            return;
        var user = args.User;

        if (ent.Comp.Tank1Slot.HasItem)
        {
            AlternativeVerb detach1 = new()
            {
                Act = () => DetachTank(ent, ent.Comp.Tank1Slot, user),
                Text = Loc.GetString("transfer-valve-detach-right-tank"),
                Priority = 1
            };
            args.Verbs.Add(detach1);
        }

        if (ent.Comp.Tank2Slot.HasItem)
        {
            AlternativeVerb detach2 = new()
            {
                Act = () => DetachTank(ent, ent.Comp.Tank2Slot, user),
                Text = Loc.GetString("transfer-valve-detach-left-tank"),
                Priority = 2
            };
            args.Verbs.Add(detach2);
        }
    }

    private void OnGetActivationVerbs(Entity<TransferValveComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanInteract)
            return;
        var user = args.User;

        if (ent.Comp.Tank1Slot.HasItem && ent.Comp.Tank2Slot.HasItem)
        {
            var toggleText = ent.Comp.ValveOpen ? "Close" : "Open";
            ActivationVerb toggle = new()
            {
                Act = () => ToggleValve(ent, user),
                Text = Loc.GetString("transfer-valve-toggle-verb", ("action", toggleText)),
                Priority = 3
            };
            args.Verbs.Add(toggle);
        }
    }

    private void DetachTank(Entity<TransferValveComponent> ent, ItemSlot tankSlot, EntityUid? user = null)
    {
        if (!tankSlot.HasItem)
            return;
        SplitGases(ent.Comp);
        if (ent.Comp.ValveOpen)
            _audioSystem.PlayPredicted(ent.Comp.CloseValveSound, ent, null);
        ent.Comp.ValveOpen = false;
        _itemSlotsSystem.SetLock(ent, tankSlot, false);
        if (user != null)
            _itemSlotsSystem.TryEjectToHands(ent, tankSlot, user);
        else
            _itemSlotsSystem.TryEject(ent, tankSlot, user, out var tank);
    }

    private void SplitGases(TransferValveComponent component)
    {
        if (!component.ValveOpen ||
            !_entManager.TryGetComponent<GasTankComponent>(component.Tank1Slot.Item, out var tank_one) ||
            !_entManager.TryGetComponent<GasTankComponent>(component.Tank2Slot.Item, out var tank_two))
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
        if (!_entManager.TryGetComponent<GasTankComponent>(component.Tank1Slot.Item, out var tank_one) ||
            !_entManager.TryGetComponent<GasTankComponent>(component.Tank2Slot.Item, out var tank_two))
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

    public void ToggleValve(Entity<TransferValveComponent> ent, EntityUid? user = null)
    {
        if (!_entManager.TryGetComponent<GasTankComponent>(ent.Comp.Tank1Slot.Item, out var tank_one) ||
            !_entManager.TryGetComponent<GasTankComponent>(ent.Comp.Tank2Slot.Item, out var tank_two))
            return;
        if (!ent.Comp.ValveOpen)
        {
            ent.Comp.ValveOpen = MergeGases(ent.Comp);
            if (ent.Comp.ValveOpen)
            {
                _audioSystem.PlayPredicted(ent.Comp.OpenValveSound, ent, null);
                var exploded = tank_two.Air.Pressure > tank_two.TankFragmentPressure;
                _adminLogger.Add(LogType.Explosion, exploded ? LogImpact.High : LogImpact.Medium,
                    $"{(user != null ? $"{ToPrettyString(user):player} caused " : "")}{ToPrettyString(ent):entity}'s valve to open{(exploded ? " resulting in an explosion." : ".")}");
            }
        }
        else
        {
            SplitGases(ent.Comp);
            ent.Comp.ValveOpen = false;
            _audioSystem.PlayPredicted(ent.Comp.CloseValveSound, ent, null);
        }
    }

    private void OnSignalReceived(Entity<TransferValveComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == ent.Comp.TogglePort && ent.Comp.NextToggle < _gameTiming.CurTime)
        {
            ent.Comp.NextToggle = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Cooldown);
            ToggleValve(ent);
            return;
        }
    }


    /// <summary>
    /// Mixes the tanks into a temporary gas mixture to calculate an approximate range of the bomb.
    /// This does actually drain both tanks irreveribly, don't use this if you plan on using the TTV later.
    /// </summary>
    /// <param name="reactionVolumeMod">Additional volume to use, if this is supposed to be mixed outside the TTV. A higher valve makes a weaker bomb</param>
    /// <param name="prereaction">Checks if the tank wouldn't fragment after a single reaction.</param>
    /// <param name="reactionCycles">How many reactions to do. Gas tanks do 3 on fragmentation.</param>
    /// <returns>The theoretical range of the bomb.</returns>
    private float CalculateBomb(Entity<TransferValveComponent> ent, float reactionVolumeMod = 0, bool prereaction = true, int reactionCycles = 3)
    {
        // Using tank 2 for fragment pressure calculations.
        if (ent.Comp.ValveOpen || !ent.Comp.Tank1Slot.Item.HasValue || !_entManager.TryGetComponent<GasTankComponent>(ent.Comp.Tank2Slot.Item, out var tank_two))
            return 0;
        GasMixture temp = new(reactionVolumeMod);
        MergeGases(ent.Comp, temp);
        // TECHNICALLY if the pressure is already greater than fragment pressure from the merge alone, a real gas tank wouldn't get this extra react,
        // but I'll still let it have it here as a treat and for parity with SS13
        if (prereaction)
        {
            _atmosphereSystem.React(temp, null);
            if (temp.Pressure < tank_two.TankFragmentPressure)
                return 0;
        }
        for (int i = 0; i < reactionCycles; i++)
        {
            _atmosphereSystem.React(temp, null);
        }
        var pressure = temp.Pressure;
        if (pressure < tank_two.TankFragmentPressure)
            return 0;
        return MathF.Sqrt((pressure - tank_two.TankFragmentPressure) / tank_two.TankFragmentScale);
    }

    private void OnFired(Entity<TransferValveComponent> ent, ref AmmoShotEvent args)
    {
        float? power = null;
        BlastWaveComponent? blastWave = null;
        foreach (var shot in args.FiredProjectiles)
        {
            if (!Resolve(shot, ref blastWave))
                continue;
            if (blastWave.Power > 0)
                continue;
            power ??= CalculateBomb(ent);
            blastWave.Power = power.Value;
        }
    }
}
