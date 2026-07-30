
using Content.Goobstation.Shared.DopplerArray;
using Content.Shared.Chat;
using Content.Shared.Explosion;
using Content.Shared.GameTicking;
using Content.Server.Chat.Systems;
using Content.Server.Power.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.DopplerArray
{
    public sealed class DopplerArraySystem : EntitySystem
    {
        [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedGameTicker _gameTicker = default!;
        [Dependency] private readonly UserInterfaceSystem _ui = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly PowerReceiverSystem _power = default!;
        private const float MaxRange = 150f;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GlobalExplosionEvent>(OnGlobalExplosion);
            SubscribeLocalEvent<DopplerArrayComponent, DopplerArrayDeleteRecord>(OnDeleteRecord);
        }

        private void OnGlobalExplosion(ref GlobalExplosionEvent args)
        {
            var nearbyDopplers = new HashSet<Entity<DopplerArrayComponent>>();
            _entityLookupSystem.GetEntitiesOnMap(args.Epicenter.MapId, nearbyDopplers);
            foreach (var ent in nearbyDopplers)
            {
                if (args.Epicenter.InRange(_transform.GetMapCoordinates(ent), ent.Comp.MaxDistance))
                    SenseExplosion(ent, ref args);
            }
        }

        private void OnDeleteRecord(Entity<DopplerArrayComponent> ent, ref DopplerArrayDeleteRecord args)
        {
            var index = (int) args.Index;
            if (index >= ent.Comp.Records.Count)
                return;
            ent.Comp.Records.RemoveAt(index);
            UpdateUserInterface(ent);
        }

        private void SenseExplosion(Entity<DopplerArrayComponent> ent, ref GlobalExplosionEvent args)
        {
            if (!IsPowered(ent))
                return;
            if (ent.Comp.NextAnnounce > _gameTiming.CurTime)
                return;
            ent.Comp.NextAnnounce = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Cooldown);
            var stationTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
            var newRecord = new TachyonRecord();
            newRecord.Name = Loc.GetString("doppler-array-log-recording", ("number", ent.Comp.RecordNumber++));
            newRecord.Timestamp = stationTime;
            newRecord.Coordinates = args.Epicenter;
            newRecord.FactualRadius = args.Intensity;
            newRecord.TheoreticalRadius = args.OrigIntensity;
            ent.Comp.Records.Add(newRecord);
            UpdateUserInterface(ent);
            _chatSystem.TrySendInGameICMessage(ent, Loc.GetString("doppler-array-explosion-detected"), InGameICChatType.Speak, false);
            //TODO: Implement 'took'
            _chatSystem.TrySendInGameICMessage(ent, Loc.GetString("doppler-array-location-and-time", ("x", MathF.Round(args.Epicenter.X)), ("y", MathF.Round(args.Epicenter.Y)), ("took", MathF.Round(0))), InGameICChatType.Speak, false);
            _chatSystem.TrySendInGameICMessage(ent, args.Intensity > args.OrigIntensity ?
                Loc.GetString("doppler-array-factual-theoretical-radius", ("factual_radius", MathF.Round(args.Intensity)), ("theoretical_radius", MathF.Round(args.OrigIntensity))) :
                Loc.GetString("doppler-array-factual-radius", ("factual_radius", MathF.Round(args.Intensity))),
                InGameICChatType.Speak, false);

        }

        private bool IsPowered(Entity<DopplerArrayComponent> ent)
        {
            return _power.IsPowered(ent);
        }

        private void UpdateUserInterface(Entity<DopplerArrayComponent> ent)
        {
            _ui.SetUiState(ent.Owner, DopplerArrayUIKey.Key, new DopplerArrayUIState(ent.Comp.Records));
        }
    }
}
