
using Content.Goobstation.Shared.Atmos.Portable.Systems;
using Content.Goobstation.Shared.Atmos.Portable;
using Content.Goobstation.Client.Atmos.UI;

namespace Content.Client.Atmos.Portable.Systems;

public sealed class PortablePumpSystem : SharedPortablePumpSystem
{
    protected override void DirtyUI(Entity<PortablePumpComponent> ent)
    {
        if (UI.TryGetOpenUi<PortablePumpBoundUserInterface>(ent.Owner, PortablePumpUiKey.Key, out var bui))
            bui.Update<PortablePumpBoundUserInterfaceState>();
    }
}
