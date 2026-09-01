
using Content.Goobstation.Shared.TransferValve.Components;
using Content.Goobstation.Shared.Weapons.Ranged;
using Content.Goobstation.Shared.Weapons.Recoil;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Weapons.Ranged;

public sealed class BlastCannonSystem : SharedBlastCannonSystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlastCannonComponent, ContainerIsInsertingAttemptEvent>(OnItemAddedAttempt);
        SubscribeLocalEvent<BlastCannonComponent, AttemptShootEvent>(OnShootAttempt);
        SubscribeLocalEvent<BlastCannonComponent, AmmoShotEvent>(OnFired);
    }

    private void OnShootAttempt(Entity<BlastCannonComponent> ent, ref AttemptShootEvent args)
    {
        if (ent.Comp.TransferValveSlot.HasItem &&
        (!EntManager.TryGetComponent<TransferValveComponent>(ent.Comp.TransferValveSlot.Item, out var transferValve) || !transferValve.Ready))
        {
            args.Cancelled = true;
            args.Message = Loc.GetString("blast-cannon-bomb-not-ready", ("ttv", Identity.Name(ent.Comp.TransferValveSlot.Item!.Value, EntManager)));
            return;
        }
    }

    private void OnFired(Entity<BlastCannonComponent> ent, ref AmmoShotEvent args)
    {
        BlastWaveComponent? blastWave = null;
        var shots = 0;
        var max_power = ent.Comp.DebugPower ?? 0;
        foreach (var shot in args.FiredProjectiles)
        {
            if (!Resolve(shot, ref blastWave))
                continue;
            shots++;
            if (ent.Comp.DebugPower != null)
                blastWave.Power = ent.Comp.DebugPower.Value;
            blastWave.HugBox = ent.Comp.HugBox;
            max_power = MathF.Max(max_power, blastWave.Power);
        }
        if (EntManager.TryGetComponent<GunRecoilComponent>(ent, out var recoil))
        {
            recoil.MassMultiplier = max_power * ent.Comp.RecoilMultiplier;
            Dirty(ent, recoil);
        }
        EntManager.TryGetComponent<GunComponent>(ent, out var gun);
        if (shots > 0)
        {
            if (gun?.Holder == null)
                _adminLogger.Add(LogType.Explosion, LogImpact.High,
                    $"{ToPrettyString(ent):entity} shot {shots} blastwave{(shots > 1 ? "s" : "")} with power {max_power}.");
            else
                _adminLogger.Add(LogType.Explosion, LogImpact.High,
                    $"{ToPrettyString(ent):entity}, fired by {ToPrettyString(gun.Holder.Value):user}, shot {shots} blastwave{(shots > 1 ? "s" : "")} with power {max_power}.");
        }
    }

    private void OnItemAddedAttempt(Entity<BlastCannonComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (args.Cancelled || !ent.Comp.Initialized || args.Container.ID != TTVSlotId)
            return;

        if (!EntManager.TryGetComponent<TransferValveComponent>(args.EntityUid, out var transferValve))
        {
            args.Cancel();
            return;
        }

        if (!transferValve.Ready)
        {
            args.Cancel();
            Popup.PopupEntity(Loc.GetString("blast-cannon-attaching-incomplete-bomb"), ent);
            return;
        }
    }
}
