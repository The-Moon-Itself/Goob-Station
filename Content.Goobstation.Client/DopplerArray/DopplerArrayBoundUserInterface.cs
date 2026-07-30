
using Content.Goobstation.Shared.DopplerArray;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.DopplerArray;

public sealed class DopplerArrayBoundUserInterface : BoundUserInterface
{
    private DopplerArrayWindow? _window;
    public DopplerArrayBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<DopplerArrayWindow>();
        _window.OnDeleteHistory += index => SendMessage(new DopplerArrayDeleteRecord(index));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not DopplerArrayUIState cast)
            return;

        _window?.UpdateState(cast);
    }
}
