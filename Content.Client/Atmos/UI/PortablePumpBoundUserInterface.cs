using Content.Shared.Atmos.Portable.Components;
using Content.Client.Atmos.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Atmos.UI;

[UsedImplicitly]
public sealed class PortablePumpBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PortablePumpWindow? _window;

    public PortablePumpBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<PortablePumpWindow>();
        _window.ToggleStatusButton.OnPressed += _ => OnToggleStatusButtonPressed();
        _window.TankEjectButton.OnPressed += _ => OnTankEjectPressed();
        _window.PumpDirectionButton.OnPressed += _ => OnTogglePumpDirectionButtonPressed();
        _window.PumpOutputPressureChanged += OnSetPressureButtonPressed;
    }

    private void OnToggleStatusButtonPressed()
    {
        if (_window == null)
            return;
        _window.SetActive(!_window.Active);
        SendMessage(new PortablePumpToggleMessage(_window.Active));
    }
    private void OnTankEjectPressed()
    {
        if (_window == null)
            return;
        SendPredictedMessage(new PortablePumpEjectTankMessage());
    }

    private void OnTogglePumpDirectionButtonPressed()
    {
        if (_window == null)
            return;
        SendPredictedMessage(new PortablePumpTogglePumpDirectionMessage());
    }

    private void OnSetPressureButtonPressed(float pressure)
    {
        if (_window == null)
            return;
        SendPredictedMessage(new PortablePumpSetPumpPressureMessage(pressure));

    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null
            || state is not PortablePumpBoundUserInterfaceState cast)
            return;
        _window.SetActive(cast.Enabled);
        _window.SetPressure(cast.Pressure);
        _window.SetConnected(cast.Connected);
        _window.SetTankPressure(cast.TankLabel, cast.TankPressure);
        _window.SetPumpDirection(cast.PumpDirection);
        _window.SetTargetPressure(cast.TargetPressure);
    }
}
