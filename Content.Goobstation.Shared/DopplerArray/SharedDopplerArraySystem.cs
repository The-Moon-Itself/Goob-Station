
using Content.Goobstation.Shared.DopplerArray;
using Content.Shared.Cargo.Components;
using Content.Shared.Chat;
using Content.Shared.Explosion;
using Content.Shared.GameTicking;
using Content.Shared.Research.Components;
using Robust.Shared.Timing;
namespace Content.Goobstation.Shared.DopplerArray
{
    public abstract class SharedDopplerArraySystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DopplerArrayComponent, DopplerArrayDeleteRecord>(OnDeleteRecord);
        }

        private void OnDeleteRecord(Entity<DopplerArrayComponent> ent, ref DopplerArrayDeleteRecord args)
        {
            var index = (int) args.Index;
            if (index >= ent.Comp.Records.Count || index < 0)
                return;
            ent.Comp.Records.RemoveAt(index);
            Dirty(ent);
            DirtyUI(ent);
        }

        protected virtual void DirtyUI(Entity<DopplerArrayComponent> ent, UserInterfaceComponent? ui=null) {}
    }
}
