
using Content.Goobstation.Shared.DopplerArray;
using Content.Shared.Explosion;
using Content.Shared.GameTicking;
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
        private const float MaxRange = 150f;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GlobalExplosionEvent>(OnGlobalExplosion);
        }

        private void OnGlobalExplosion(ref GlobalExplosionEvent args)
        {
            var nearbyDopplers = _entityLookupSystem.GetEntitiesInRange<DopplerArrayComponent>(args.Epicenter, MaxRange);
            foreach (var ent in nearbyDopplers)
            {
                SenseExplosion(ent, ref args);
            }
        }

        private void SenseExplosion(Entity<DopplerArrayComponent> ent, ref GlobalExplosionEvent args)
        {
            var stationTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
            var newRecord = new TachyonRecord(Loc.GetString("doppler-array-log-recording", ("number", ent.Comp.RecordNumber++)), stationTime, args.Epicenter, args.Intensity, args.OrigIntensity);
            ent.Comp.Records.Add(newRecord);
            UpdateUserInterface(ent);
        }

        private void UpdateUserInterface(Entity<DopplerArrayComponent> ent)
        {
            _ui.SetUiState(ent.Owner, DopplerArrayUIKey.Key, new DopplerArrayUIState(ent.Comp.Records));
        }
    }
}
