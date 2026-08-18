
using Content.Goobstation.Shared.DopplerArray;
using Content.Shared.Research.Components;
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
        _window.OnDeleteHistory += index => SendPredictedMessage(new DopplerArrayDeleteRecord(index));
        _window.OnServerButtonPressed += () =>
        {
            SendPredictedMessage(new ConsoleServerSelectionMessage());
        };

        Update();
    }

    public override void Update()
    {
        if (_window == null || !EntMan.TryGetComponent(Owner, out DopplerArrayComponent? doppler))
            return;

        _window.ClearRecords();
        foreach (TachyonRecord entry in doppler.Records)
        {
            _window.AddRecord(entry);
        }

        _window.SetResearchEnabled(doppler.ResearchEnabled);

    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not DopplerArrayUIState cast)
            return;

        _window?.UpdateState(cast);
    }
}
