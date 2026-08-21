
using Content.Goobstation.Shared.Atmos.Portable;
using Content.Goobstation.Client.Atmos.UI;

namespace Content.Goobstation.Client.Atmos.Portable.Systems;

public sealed class PortableScrubberSystem : SharedPortableScrubberSystem
{
    protected override void DirtyUI(Entity<PortableScrubberComponent> ent)
    {
        if (UI.TryGetOpenUi<PortableScrubberBoundUserInterface>(ent.Owner, PortableScrubberUiKey.Key, out var bui))
            bui.Update<PortableScrubberBoundUserInterfaceState>();
    }
}
