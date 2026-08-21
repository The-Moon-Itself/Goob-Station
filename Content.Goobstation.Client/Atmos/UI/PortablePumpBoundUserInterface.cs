
using Content.Client.Power.EntitySystems;
using Content.Goobstation.Shared.Atmos.Portable;
using Content.Shared.Power.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Atmos.UI;

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
        _window.PumpPressureOutputInput.OnValueChanged += args => OnPressureValueChanged(args.Value);
        _window.MaxOutputPressureButton.OnPressed += _ => OnMaxPressureButotnPressed();
        _window.PumpDirectionButton.OnPressed += _ => OnTogglePumpDirectionButtonPressed();

        Update();
    }

    public override void Update()
    {
        base.Update();

        if (_window == null || !EntMan.TryGetComponent(Owner, out PortablePumpComponent? pump))
            return;

        var receiverSys = EntMan.System<PowerReceiverSystem>();
        SharedApcPowerReceiverComponent? receiver = null;

        if (receiverSys.ResolveApc(Owner, ref receiver))
        {
            _window.SetActive(!receiver.PowerDisabled);
        }
        _window.SetPumpDirection(pump.PumpDirection);
        _window.SetTargetPressure(pump.TargetPressure);
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

    private void OnPressureValueChanged(float pressure)
    {
        if (_window == null)
            return;
        _window.OutputPressure = pressure;
        SendPredictedMessage(new PortablePumpSetPumpPressureMessage(pressure));

    }

    private void OnMaxPressureButotnPressed()
    {
        if (_window == null || !EntMan.TryGetComponent(Owner, out PortablePumpComponent? pump))
            return;
        _window.PumpPressureOutputInput.Value = pump.MaximumPressure;
        OnPressureValueChanged(pump.MaximumPressure);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null
            || state is not PortablePumpBoundUserInterfaceState cast)
            return;
        _window.SetPressure(cast.Pressure);
        _window.SetConnected(cast.Connected);
        _window.SetTankPressure(cast.TankLabel, cast.TankPressure);
    }
}
