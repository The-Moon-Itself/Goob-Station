using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Content.Client.Power.EntitySystems;
using Content.Shared.Power.Components;
using Content.Goobstation.Shared.Atmos.Portable;

namespace Content.Goobstation.Client.Atmos.UI;

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

        _window.ToggleStatusButton.OnPressed += _ => SendPredictedMessage(new PortableScrubberToggleMessage());
        _window.ScrubberFilterGasChanged += toggledGas => SendPredictedMessage(new PortableScrubberFilterGasToggleMessage(toggledGas));
        _window.TankEjectButton.OnPressed += _ => SendPredictedMessage(new PortableScrubberEjectTankMessage());

        Update();
    }

    public override void Update()
    {
        base.Update();

        if (_window == null || !EntMan.TryGetComponent(Owner, out PortableScrubberComponent? scrubber))
            return;

        var receiverSys = EntMan.System<PowerReceiverSystem>();
        SharedApcPowerReceiverComponent? receiver = null;

        if (receiverSys.ResolveApc(Owner, ref receiver))
        {
            _window.SetActive(!receiver.PowerDisabled);
        }
        _window.SetFilterGases(scrubber.FilterGases);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null
            || state is not PortableScrubberBoundUserInterfaceState cast)
            return;
        _window.SetPressure(cast.Pressure, cast.IsFull);
        _window.SetConnected(cast.Connected);
        _window.SetTankPressure(cast.TankLabel, cast.TankPressure);

    }
}
