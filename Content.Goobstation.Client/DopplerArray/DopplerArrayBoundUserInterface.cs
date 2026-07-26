
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
    }
}
