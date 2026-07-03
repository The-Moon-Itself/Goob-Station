
using Content.Client.Atmos.UI;
using Content.Shared.Atmos.Portable;
using Content.Shared.Atmos.Portable.Systems;
using Content.Shared.Atmos.Portable.Components;

namespace Content.Client.Atmos.Portable.Systems;

public sealed class PortableScrubberSystem : SharedPortableScrubberSystem
{
    protected override void DirtyUI(Entity<PortableScrubberComponent> ent)
    {
        if (UI.TryGetOpenUi<PortableScrubberBoundUserInterface>(ent.Owner, PortableScrubberUiKey.Key, out var bui))
            bui.Update<PortableScrubberBoundUserInterfaceState>();
    }
}
