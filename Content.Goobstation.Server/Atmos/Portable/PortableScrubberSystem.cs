// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared.Examine;
using Content.Shared.Destructible;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.EntitySystems;
using Robust.Server.GameObjects;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.Audio;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Database;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Content.Shared.Atmos.Components;
using Content.Shared.IdentityManagement;
using Content.Goobstation.Shared.Atmos.Portable;

namespace Content.Goobstation.Server.Atmos.Portable;

public sealed class PortableScrubberSystem : SharedPortableScrubberSystem
{
    [Dependency] private readonly GasVentScrubberSystem _scrubberSystem = default!;
    [Dependency] private readonly GasCanisterSystem _canisterSystem = default!;
    [Dependency] private readonly GasPortableSystem _gasPortableSystem = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly AmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PortableScrubberComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        SubscribeLocalEvent<PortableScrubberComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<PortableScrubberComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<PortableScrubberComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PortableScrubberComponent, DestructionEventArgs>(OnDestroyed);
        SubscribeLocalEvent<PortableScrubberComponent, GasAnalyzerScanEvent>(OnScrubberAnalyzed);

        SubscribeLocalEvent<PortableScrubberComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);
    }

    private bool IsFull(PortableScrubberComponent component)
    {
        return component.Air.Pressure >= component.MaxPressure;
    }

    private void OnDeviceUpdated(Entity<PortableScrubberComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        var timeDelta = args.dt;

        if (!Power.IsPowered(ent.Owner))
        {
            return;
        }

        // If we are on top of a connector port, empty into it.
        if (_nodeContainer.TryGetNode(ent.Owner, ent.Comp.PortName, out PortablePipeNode? portableNode)
            && portableNode.ConnectionsEnabled)
        {
            _atmosphereSystem.React(ent.Comp.Air, portableNode);
            if (portableNode.NodeGroup is PipeNet {NodeCount: > 1} net)
                _canisterSystem.MixContainerWithPipeNet(ent.Comp.Air, net.Air);
        }

        if (IsFull(ent))
        {
            UpdateAppearance(ent, true, false);
            DirtyUI(ent);
            return;
        }

        if (args.Grid is not {} grid)
        {
            DirtyUI(ent);
            return;
        }

        var position = _transformSystem.GetGridTilePositionOrDefault(ent.Owner);
        var environment = _atmosphereSystem.GetTileMixture(grid, args.Map, position, true);

        GasTankComponent? gasTank = null;
        var hasTank = (ent.Comp.GasTankSlot?.HasItem ?? false) && _entManager.TryGetComponent<GasTankComponent>(ent.Comp.GasTankSlot.Item!.Value, out gasTank);

        var running = Scrub(timeDelta, ent.Comp, (gasTank == null) ? environment : gasTank.Air);

        UpdateAppearance(ent, false, running);
        // We scrub once to see if we can and set the animation
        if (!running || hasTank)
        {
            DirtyUI(ent);
            return;
        }

        // widenet
        var enumerator = _atmosphereSystem.GetAdjacentTileMixtures(grid, position, false, true);
        while (enumerator.MoveNext(out var adjacent))
        {
            Scrub(timeDelta, ent.Comp, adjacent);
        }
        DirtyUI(ent);
    }

    /// <summary>
    /// If there is a port under us, let us connect with adjacent atmos pipes.
    /// </summary>
    private void OnAnchorChanged(Entity<PortableScrubberComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!_nodeContainer.TryGetNode(ent.Owner, ent.Comp.PortName, out PipeNode? portableNode))
            return;

        portableNode.ConnectionsEnabled = (args.Anchored && _gasPortableSystem.FindGasPortIn(Transform(ent.Owner).GridUid, Transform(ent.Owner).Coordinates, out _));

        Appearance.SetData(ent, PortableScrubberVisuals.IsDraining, portableNode.ConnectionsEnabled);
        DirtyUI(ent);
    }

    private void OnPowerChanged(Entity<PortableScrubberComponent> ent, ref PowerChangedEvent args)
    {
        UpdateAppearance(ent, IsFull(ent), args.Powered);
        DirtyUI(ent);
    }

    /// <summary>
    /// Examining tells you how full it is as a %.
    /// </summary>
    private void OnExamined(EntityUid uid, PortableScrubberComponent component, ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
        {
            var percentage = Math.Round(((component.Air.Pressure) / component.MaxPressure) * 100);
            args.PushMarkup(Loc.GetString("portable-scrubber-fill-level", ("percent", percentage)));
        }
    }

    /// <summary>
    /// Give the GUI its starting information.
    /// </summary>
    private void OnBeforeOpened(Entity<PortableScrubberComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        DirtyUI(ent);
    }

    /// <summary>
    /// When this is destroyed, we dump out all the gas inside.
    /// </summary>
    private void OnDestroyed(EntityUid uid, PortableScrubberComponent component, DestructionEventArgs args)
    {
        var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);

        if (environment != null)
            _atmosphereSystem.Merge(environment, component.Air);

        AdminLogger.Add(LogType.CanisterPurged, LogImpact.Medium, $"Portable scrubber {ToPrettyString(uid):canister} purged its contents of {component.Air} into the environment.");
        component.Air.Clear();
    }

    private bool Scrub(float timeDelta, PortableScrubberComponent scrubber, GasMixture? tile)
    {
        return _scrubberSystem.Scrub(timeDelta, scrubber.TransferRate * _atmosphereSystem.PumpSpeedup(), ScrubberPumpDirection.Scrubbing, scrubber.FilterGases, tile, scrubber.Air);
    }

    protected override void OnToggle(Entity<PortableScrubberComponent> ent, ref PortableScrubberToggleMessage args)
    {
        base.OnToggle(ent, ref args);
        UpdateAppearance(ent, IsFull(ent), Power.IsPowered(ent.Owner));
    }

    protected override void DirtyUI(Entity<PortableScrubberComponent> ent)
    {
        if (!Slots.TryGetSlot(ent, ent.Comp.ContainerName, out var slot))
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
        _userInterfaceSystem.SetUiState(ent.Owner, PortableScrubberUiKey.Key,
            new PortableScrubberBoundUserInterfaceState(ent.Comp.Air.Pressure, IsFull(ent), connected, tankLabel, tankPressure));
    }

    private void UpdateAppearance(EntityUid uid, bool isFull, bool isRunning)
    {
        _ambientSound.SetAmbience(uid, isRunning);

        Appearance.SetData(uid, PortableScrubberVisuals.IsFull, isFull);
        Appearance.SetData(uid, PortableScrubberVisuals.IsRunning, isRunning);
    }

    /// <summary>
    /// Returns the gas mixture for the gas analyzer
    /// </summary>
    private void OnScrubberAnalyzed(EntityUid uid, PortableScrubberComponent component, GasAnalyzerScanEvent args)
    {
        args.GasMixtures ??= new List<(string, GasMixture?)>();
        args.GasMixtures.Add((Name(uid), component.Air));
    }
}
