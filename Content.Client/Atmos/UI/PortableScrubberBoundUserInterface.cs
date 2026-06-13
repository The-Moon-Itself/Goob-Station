using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos;

namespace Content.Client.Atmos.UI;

[UsedImplicitly]
public sealed class PortableScrubberBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PortableScrubberWindow? _window;

    public PortableScrubberBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<PortableScrubberWindow>();

        _window.ToggleStatusButton.OnPressed += _ => OnToggleStatusButtonPressed();
        _window.ScrubberFilterGasChanged += OnFilterGasToggled;
        _window.TankEjectButton.OnPressed += _ => OnTankEjectPressed();
    }

    private void OnToggleStatusButtonPressed()
    {
        if (_window == null)
            return;
        _window.SetActive(!_window.Active);
        SendMessage(new PortableScrubberToggleMessage(_window.Active));
    }

    private void OnTankEjectPressed()
    {
        if (_window == null)
            return;

        SendMessage(new PortableScrubberEjectTankMessage());
    }

    private void OnFilterGasToggled(Gas toggledGas)
    {
        if (_window == null)
            return;
        SendMessage(new PortableScrubberFilterGasToggleMessage(toggledGas));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null
            || state is not PortableScrubberBoundUserInterfaceStatusState cast)
            return;
        _window.SetActive(cast.Enabled);
        _window.SetPressure(cast.Pressure, cast.IsFull);
        _window.SetConnected(cast.Connected);
        _window.SetFilterGases(cast.FilterGases);
        _window.SetTankPressure(cast.TankLabel, cast.TankPressure);

    }
}
