
using Content.Client.Atmos.UI;
using Content.Shared.Atmos.Portable;
using Content.Shared.Atmos.Portable.Systems;
using Content.Shared.Atmos.Portable.Components;

namespace Content.Client.Atmos.Portable.Systems;

public sealed class PortablePumpSystem : SharedPortablePumpSystem
{
    protected override void DirtyUI(Entity<PortablePumpComponent> ent)
    {
        if (UI.TryGetOpenUi<PortablePumpBoundUserInterface>(ent.Owner, PortableScrubberUiKey.Key, out var bui))
            bui.Update<PortablePumpBoundUserInterfaceState>();
    }
}
