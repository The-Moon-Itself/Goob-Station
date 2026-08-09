using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Portable;
using Content.Shared.Atmos.Portable.Components;
using Content.Shared.Atmos.Portable.Systems;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Content.Shared.Power;
using Content.Shared.Destructible;

namespace Content.Server.Atmos.Portable;

public sealed class PortablePumpSystem : SharedPortablePumpSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly GasCanisterSystem _canisterSystem = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly GasPortableSystem _gasPortableSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PortablePumpComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        SubscribeLocalEvent<PortablePumpComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<PortablePumpComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<PortablePumpComponent, DestructionEventArgs>(OnDestroyed);

        SubscribeLocalEvent<PortablePumpComponent, GasAnalyzerScanEvent>(OnScrubberAnalyzed);

        SubscribeLocalEvent<PortablePumpComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);
        SubscribeLocalEvent<PortablePumpComponent, PortablePumpToggleMessage>(OnToggle);
    }

    private void OnDeviceUpdated(Entity<PortablePumpComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        if (_nodeContainer.TryGetNode(ent.Owner, ent.Comp.PortName, out PortablePipeNode? portableNode)
            && portableNode.ConnectionsEnabled)
        {
            _atmosphereSystem.React(ent.Comp.Air, portableNode);
            if (portableNode.NodeGroup is PipeNet { NodeCount: > 1 } net)
                _canisterSystem.MixContainerWithPipeNet(ent.Comp.Air, net.Air);
        }

        var powered = _power.IsPowered(ent);
        Appearance.SetData(ent, PortablePumpVisuals.IsRunning, powered);
        if (!powered)
        {
            DirtyUI(ent);
            return;
        }

        if (args.Grid is not { } grid)
        {
            DirtyUI(ent);
            return;
        }

        var position = _transformSystem.GetGridTilePositionOrDefault(ent.Owner);
        var environment = _atmosphereSystem.GetTileMixture(grid, args.Map, position, true);
        GasTankComponent? gasTank = null;
        var hasTank = (ent.Comp.GasTankSlot?.HasItem ?? false) && _entManager.TryGetComponent<GasTankComponent>(ent.Comp.GasTankSlot.Item!.Value, out gasTank);

        GasMixture? other = (gasTank == null) ? environment : gasTank.Air;
        if (other == null)
        {
            DirtyUI(ent);
            return;
        }
        GasMixture sending = (ent.Comp.PumpDirection == VentPumpDirection.Siphoning) ? other : ent.Comp.Air;
        GasMixture receiving = (ent.Comp.PumpDirection == VentPumpDirection.Siphoning) ? ent.Comp.Air : other;
        Appearance.SetData(ent, PortablePumpVisuals.IsFull, !_atmosphereSystem.PumpGasTo(sending, receiving, ent.Comp.TargetPressure));
        DirtyUI(ent);
    }

    private void OnAnchorChanged(Entity<PortablePumpComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!_nodeContainer.TryGetNode(ent.Owner, ent.Comp.PortName, out PipeNode? portableNode))
            return;

        portableNode.ConnectionsEnabled = (args.Anchored && _gasPortableSystem.FindGasPortIn(Transform(ent.Owner).GridUid, Transform(ent.Owner).Coordinates, out _));

        Appearance.SetData(ent, PortablePumpVisuals.IsConnected, portableNode.ConnectionsEnabled);
        DirtyUI(ent);
    }

    private void OnPowerChanged(Entity<PortablePumpComponent> ent, ref PowerChangedEvent args)
    {
        Appearance.SetData(ent, PortablePumpVisuals.IsRunning, false);
        DirtyUI(ent);
    }

    private void OnDestroyed(Entity<PortablePumpComponent> ent, ref DestructionEventArgs args)
    {
        var environment = _atmosphereSystem.GetContainingMixture(ent.Owner, false, true);

        if (environment != null)
            _atmosphereSystem.Merge(environment, ent.Comp.Air);

        _adminLogger.Add(LogType.CanisterPurged, LogImpact.Medium, $"Portable pump {ToPrettyString(ent.Owner):canister} purged its contents of {ent.Comp.Air} into the environment.");
        ent.Comp.Air.Clear();
    }

    private void OnScrubberAnalyzed(Entity<PortablePumpComponent> ent, ref GasAnalyzerScanEvent args)
    {
        args.GasMixtures ??= new List<(string, GasMixture?)>();
        args.GasMixtures.Add((Name(ent.Owner), ent.Comp.Air));
    }

    private void OnBeforeOpened(Entity<PortablePumpComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        DirtyUI(ent);
    }

    private void OnToggle(Entity<PortablePumpComponent> ent, ref PortablePumpToggleMessage args)
    {
        ApcPowerReceiverComponent? powerReceiver = null;
        if (!Resolve(ent, ref powerReceiver))
            return;

        _power.TogglePower(ent);

        Appearance.SetData(ent, PortablePumpVisuals.IsRunning, _power.IsPowered(ent));
        DirtyUI(ent);
    }

    protected override void DirtyUI(Entity<PortablePumpComponent> ent)
    {
        if (!TryComp<ApcPowerReceiverComponent>(ent, out var powerReceiver)
            || !Slots.TryGetSlot(ent, ent.Comp.ContainerName, out var slot))
        {
            return;
        }
        var connected = _nodeContainer.TryGetNode(ent.Owner, ent.Comp.PortName, out PortablePipeNode? portableNode)
                        && portableNode.ConnectionsEnabled
                        && portableNode.NodeGroup is PipeNet { NodeCount: > 1 };

        string? tankLabel = null;
        var tankPressure = 0f;

        if ((slot?.HasItem ?? false) && _entManager.TryGetComponent<GasTankComponent>(slot.Item!.Value, out var gasTank))
        {
            tankLabel = Identity.Name(slot.Item.Value, _entManager);
            tankPressure = gasTank.Air.Pressure;
        }
        UI.SetUiState(ent.Owner, PortablePumpUiKey.Key,
            new PortablePumpBoundUserInterfaceState(!powerReceiver.PowerDisabled, ent.Comp.Air.Pressure, connected, ent.Comp.PumpDirection, ent.Comp.TargetPressure, tankLabel, tankPressure));
    }
}
