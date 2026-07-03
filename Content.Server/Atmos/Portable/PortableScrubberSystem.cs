// SPDX-FileCopyrightText: 2022 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2022 Francesco <frafonia@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos.Portable;
using Content.Shared.Atmos.Visuals;
using Content.Shared.Examine;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Destructible;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Portable.Systems;
using Robust.Shared.Containers;
using Robust.Server.GameObjects;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.Audio;
using Content.Server.Administration.Logs;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Database;
using Content.Shared.Power;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.UserInterface;
using Content.Shared.Atmos.Portable.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.IdentityManagement;

namespace Content.Server.Atmos.Portable
{
    public sealed class PortableScrubberSystem : SharedPortableScrubberSystem
    {
        [Dependency] private readonly GasVentScrubberSystem _scrubberSystem = default!;
        [Dependency] private readonly GasCanisterSystem _canisterSystem = default!;
        [Dependency] private readonly GasPortableSystem _gasPortableSystem = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly PowerReceiverSystem _power = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
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
            SubscribeLocalEvent<PortableScrubberComponent, PortableScrubberToggleMessage>(OnToggle);
            SubscribeLocalEvent<PortableScrubberComponent, PortableScrubberFilterGasToggleMessage>(OnFilterGasToggled);
        }

        private bool IsFull(PortableScrubberComponent component)
        {
            return component.Air.Pressure >= component.MaxPressure;
        }

        private void OnDeviceUpdated(Entity<PortableScrubberComponent> ent, ref AtmosDeviceUpdateEvent args)
        {
            var timeDelta = args.dt;

            //Theoretically no UI change here, we don't show powered, just if switched on.
            if (!_power.IsPowered(ent))
            {
                DirtyUI(ent);
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
        /// Toggle the scrubber's power.
        /// </summary>
        private void OnToggle(Entity<PortableScrubberComponent> ent, ref PortableScrubberToggleMessage args)
        {
            ApcPowerReceiverComponent? powerReceiver = null;
            if (!Resolve(ent, ref powerReceiver))
                return;

            _power.TogglePower(ent);

            UpdateAppearance(ent, IsFull(ent), _power.IsPowered(ent));
            DirtyUI(ent);
        }


        /// <summary>
        /// Toggle whether a gas is being scrubbed
        /// </summary>
        private void OnFilterGasToggled(Entity<PortableScrubberComponent> ent, ref PortableScrubberFilterGasToggleMessage args)
        {
            if (!ent.Comp.FilterGases.Add(args.ToggledGas))
                ent.Comp.FilterGases.Remove(args.ToggledGas);
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

            _adminLogger.Add(LogType.CanisterPurged, LogImpact.Medium, $"Portable scrubber {ToPrettyString(uid):canister} purged its contents of {component.Air} into the environment.");
            component.Air.Clear();
        }

        private bool Scrub(float timeDelta, PortableScrubberComponent scrubber, GasMixture? tile)
        {
            return _scrubberSystem.Scrub(timeDelta, scrubber.TransferRate * _atmosphereSystem.PumpSpeedup(), ScrubberPumpDirection.Scrubbing, scrubber.FilterGases, tile, scrubber.Air);
        }

        protected override void DirtyUI(Entity<PortableScrubberComponent> ent)
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
            _userInterfaceSystem.SetUiState(ent.Owner, PortableScrubberUiKey.Key,
                new PortableScrubberBoundUserInterfaceState(!powerReceiver.PowerDisabled, ent.Comp.Air.Pressure, IsFull(ent), connected, ent.Comp.FilterGases, tankLabel, tankPressure));
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
}
