
using Content.Goobstation.Shared.DopplerArray;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.DopplerArray;

public sealed class DopplerArraySystem : SharedDopplerArraySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DopplerArrayComponent, AfterAutoHandleStateEvent>(OnAfterState);
    }

    private void OnAfterState(Entity<DopplerArrayComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        DirtyUI(ent);
    }

    protected override void DirtyUI(Entity<DopplerArrayComponent> ent, UserInterfaceComponent? ui = null)
    {
        if (_ui.TryGetOpenUi<DopplerArrayBoundUserInterface>(ent.Owner, DopplerArrayUIKey.Key, out var bui))
        {
            bui.Update();
        }
    }
}
