
using Content.Goobstation.Shared.DopplerArray;
using Content.Shared.Cargo.Components;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Research.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.EntitySystems;
using Content.Server.Research.Systems;
using Content.Server.Station.Systems;
using Robust.Shared.Timing;
using Content.Goobstation.Shared.Explosion;
namespace Content.Goobstation.Server.DopplerArray
{
    public sealed class DopplerArraySystem : SharedDopplerArraySystem
    {
        [Dependency] private readonly CargoSystem _cargo = default!;
        [Dependency] private readonly SharedChatSystem _chatSystem = default!;
        [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly ExplosionSystem _explosion = default!;
        [Dependency] private readonly SharedGameTicker _gameTicker = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly PowerReceiverSystem _power = default!;
        [Dependency] private readonly ResearchSystem _research = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;

        // The budget of whatever client CC is selling bomb recipes to.
        // Should only be set at round start.
        private int _toxinsPayoutBudget;
        // Which dopplers are actively announcing detected explosions
        private readonly Dictionary<Entity<DopplerArrayComponent>, float> _speakingDopplers = new();
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GlobalExplosionEvent>(OnGlobalExplosion);
            // Total money that can be paid out by doppler arrays in a single round.
            _toxinsPayoutBudget = 500000;
        }
        private void PopMessage(Entity<DopplerArrayComponent> ent)
        {
            if (ent.Comp.MessageBuffer.TryDequeue(out var message))
            {
                _chatSystem.TrySendInGameICMessage(ent, message, InGameICChatType.Speak, false);
                _speakingDopplers[ent] = ent.Comp.AnnouceCooldown;
            }
            else
                _speakingDopplers.Remove(ent);
        }

        private void QueueMessage(Entity<DopplerArrayComponent> ent, string message)
        {
            ent.Comp.MessageBuffer.Enqueue(message);
            if (!_speakingDopplers.ContainsKey(ent))
                _speakingDopplers[ent] = ent.Comp.AnnouceCooldown;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var (ent, time) in _speakingDopplers)
            {
                if (time <= frameTime)
                    PopMessage(ent);
                else
                    _speakingDopplers[ent] = time - frameTime;
            }
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

        private void SenseExplosion(Entity<DopplerArrayComponent> ent, ref GlobalExplosionEvent args)
        {
            if (!IsPowered(ent))
                return;
            if (ent.Comp.NextSense > _gameTiming.CurTime)
                return;
            ent.Comp.NextSense = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Cooldown);
            var stationTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
            var newRecord = new TachyonRecord()
            {
                Name = Loc.GetString("doppler-array-log-recording", ("number", ent.Comp.RecordNumber++)),
                Timestamp = stationTime,
                Coordinates = args.Epicenter,
                FactualRadius = _explosion.IntensityToRadius(args.Intensity, args.IntensitySlope, args.MaxIntensity),
                TheoreticalRadius = _explosion.IntensityToRadius(args.OrigIntensity, args.IntensitySlope, args.MaxIntensity)
            };
            ent.Comp.Records.Add(newRecord);
            Dirty(ent);
            QueueMessage(ent, Loc.GetString("doppler-array-explosion-detected"));
            QueueMessage(ent, Loc.GetString("doppler-array-location", ("x", MathF.Round(args.Epicenter.X)), ("y", MathF.Round(args.Epicenter.Y))));
            QueueMessage(ent, args.OrigIntensity > args.Intensity ?
                Loc.GetString("doppler-array-factual-theoretical-radius", ("factual_radius", MathF.Round(newRecord.FactualRadius)), ("theoretical_radius", MathF.Round(newRecord.TheoreticalRadius))) :
                Loc.GetString("doppler-array-factual-radius", ("factual_radius", MathF.Round(newRecord.FactualRadius))));
            CalculatePayout(ent, newRecord);
        }

        private void CalculatePayout(Entity<DopplerArrayComponent> ent, TachyonRecord record)
        {
            // Check if this is doppler does payouts
            if (!ent.Comp.ResearchEnabled && !ent.Comp.ProfitEnabled)
                return;

            // Prevent things like C4 or small chem bombs from giving science money.
            if (record.TheoreticalRadius < ent.Comp.PayoutRequiredRadius)
            {
                QueueMessage(ent, Loc.GetString("doppler-array-research-below-minimum"));
                return;
            }

            int researchPayout = 0;
            int profitPayout = 0;
            // Get research server if we have one and account for existing toxins research
            if (_entManager.TryGetComponent<ResearchClientComponent>(ent, out var client) && client.Server.HasValue && ent.Comp.ResearchEnabled)
                researchPayout = -1 * _research.GetServerPointsByType(client.Server.Value, ResearchServerPointSources.Toxins);

            // y = x/(x+b) -> yx+yb = x -> yb = x(1-y) -> b = x(1-y)/y
            // y = 0.9, x = RiseTime -> RiseTime * (1-0.9)/0.9 = RiseTime*0.1111
            var b = ent.Comp.ResearchRiseTime * 0.1111f;
            researchPayout += (int) Math.Round(ent.Comp.MaxResearchPayout * record.TheoreticalRadius / (record.TheoreticalRadius + b));

            // Abort if no payout
            if (researchPayout <= 0)
            {
                QueueMessage(ent, Loc.GetString("doppler-array-research-not-peak"));
                return;
            }

            if (ent.Comp.ResearchEnabled && (client?.Server.HasValue ?? false))
                _research.ModifyServerPoints(client.Server.Value, researchPayout, null, ResearchServerPointSources.Toxins);
            else if (ent.Comp.ResearchEnabled && client != null)
                QueueMessage(ent, Loc.GetString("doppler-array-research-missing-server"));

            var stationUid = _station.GetOwningStation(ent);
            if (ent.Comp.ProfitEnabled && TryComp(stationUid, out StationBankAccountComponent? bank))
            {
                profitPayout = Math.Min((int) (ent.Comp.ProfitMultiplier * researchPayout), _toxinsPayoutBudget);
                _toxinsPayoutBudget -= profitPayout;
                _cargo.UpdateBankAccount((stationUid.Value, bank), profitPayout, ent.Comp.LinkedAccount);
            }

            string payout_loc_string;
            // We can only be here if researchPayout is > 0, so we only need to check ResearchEnabled to see if any were awarded.
            // Likewise, profitPayout will only be non-zero if ProfitEnable is true, so we only need to check the former.
            switch ((ent.Comp.ResearchEnabled ? 0b1 : 0) + (profitPayout > 0 ? 0b10 : 0))
            {
                case 1: // Research only
                    payout_loc_string = "doppler-array-research-points-generated";
                    break;
                case 2: // Profit only
                    payout_loc_string = "doppler-array-research-profit-generated";
                    break;
                case 3: // Both
                    payout_loc_string = "doppler-array-research-points-and-profit-generated";
                    break;
                default: // Somehow neither
                    return;
            }
            QueueMessage(ent, Loc.GetString(payout_loc_string, ("research", researchPayout), ("profit", profitPayout)));
        }

        private bool IsPowered(Entity<DopplerArrayComponent> ent)
        {
            return _power.IsPowered(ent);
        }
    }
}
