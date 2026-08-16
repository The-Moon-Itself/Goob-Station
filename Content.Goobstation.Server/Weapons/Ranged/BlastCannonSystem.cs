
using Content.Goobstation.Shared.TransferValve.Components;
using Content.Goobstation.Shared.Weapons.Ranged;
using Content.Shared.IdentityManagement;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Weapons.Ranged;

public sealed class BlastCannonSystem : SharedBlastCannonSystem
{
    protected override void OnShootAttempt(Entity<BlastCannonComponent> ent, ref AttemptShootEvent args)
    {
        if (!ent.Comp.TransferValveSlot.HasItem)
            return;
        if (!EntManager.TryGetComponent<TransferValveComponent>(ent.Comp.TransferValveSlot.Item, out var transferValve) || !transferValve.Ready)
        {
            args.Cancelled = true;
            args.Message = Loc.GetString("blast-cannon-bomb-not-ready", ("ttv", Identity.Name(ent.Comp.TransferValveSlot.Item!.Value, EntManager)));
        }
    }

    protected override void OnFired(Entity<BlastCannonComponent> ent, ref AmmoShotEvent args)
    {
        BlastWaveComponent? blastWave = null;
        foreach (var shot in args.FiredProjectiles)
        {
            if (!Resolve(shot, ref blastWave))
                continue;
            if (ent.Comp.DebugPower != null)
                blastWave.Power = ent.Comp.DebugPower.Value;
            blastWave.HugBox = ent.Comp.HugBox;
        }
    }

    protected override void OnItemAddedAttempt(Entity<BlastCannonComponent> ent, ref ContainerIsInsertingAttemptEvent args)
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
