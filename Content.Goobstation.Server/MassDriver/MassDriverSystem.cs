
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared._White.Grab;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.IdentityManagement;
using Content.Shared.Power.Components;
using Content.Goobstation.Shared.MassDriver;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;
using Content.Shared.Wires;

namespace Content.Goobstation.Server.MassDriver;

public sealed class MassDriverSystem : EntitySystem
{

    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;

    private int _nextAnimationNumber = 0;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MassDriverComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<MassDriverComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnComponentInit(Entity<MassDriverComponent> ent, ref ComponentInit args)
    {
        _signalSystem.EnsureSinkPorts(ent.Owner, ent.Comp.LaunchPort);
    }

    private void OnSignalReceived(Entity<MassDriverComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == ent.Comp.LaunchPort)
            Drive(ent);
    }

    private void Drive(Entity<MassDriverComponent> ent)
    {
        if (ent.Comp.ReadyWhen > _gameTiming.CurTime)
            return;
        if (!_entManager.TryGetComponent<BatteryComponent>(ent, out var batteryComp))
            return;
        var battery = (ent, batteryComp);
        if (_battery.GetCharge(battery) < ent.Comp.PowerPerObject)
            return;
        if (TryComp<WiresPanelComponent>(ent, out var wiresPanel) && wiresPanel.Open)
            return;

        // Play animation
        if (TryComp<AppearanceComponent>(ent, out var appearance))
            _appearance.SetData(ent, MassDriverVisuals.Active, ++_nextAnimationNumber, appearance);

        ent.Comp.ReadyWhen = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Cooldown);
        var throwTarget = new EntityCoordinates(ent, ent.Comp.TargetAngle.ToWorldVec() * ent.Comp.DriveRange);
        var throwDir = _transform.ToMapCoordinates(throwTarget).Position - _transform.GetMapCoordinates(ent).Position;
        HashSet<EntityUid> intersecting = new();
        // one billion overloads and SOMEHOW this is the only usable one.
        _entityLookupSystem.GetEntitiesIntersecting(ent, intersecting);
        int itemsThrown = 0;
        foreach (EntityUid obj in intersecting)
        {
            if (!TryComp<PhysicsComponent>(obj, out var physics))
                continue;
            if (itemsThrown >= ent.Comp.ObjectLimit || !_battery.TryUseCharge(battery, ent.Comp.PowerPerObject))
            {
                _popup.PopupEntity(Loc.GetString("mass-driver-jammed", ("driver", Identity.Name(ent, _entManager))), ent);
                break;
            }
            _grabThrown.Throw(obj, ent, throwDir, ent.Comp.Power);
            itemsThrown++;
        }
    }
}
