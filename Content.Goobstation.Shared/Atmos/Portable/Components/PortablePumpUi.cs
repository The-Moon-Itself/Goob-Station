using Content.Shared.Atmos.Piping.Unary.Components;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Atmos.Portable;

[Serializable]
[NetSerializable]
public enum PortablePumpUiKey
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class PortablePumpBoundUserInterfaceState : BoundUserInterfaceState
{
    public float Pressure { get; }
    public bool Connected { get; }
    public string? TankLabel;
    public float TankPressure;

    public PortablePumpBoundUserInterfaceState(float pressure, bool connected, string? tankLabel, float tankPressure)
    {
        Pressure = pressure;
        Connected = connected;
        TankLabel = tankLabel;
        TankPressure = tankPressure;
    }
}

[Serializable]
[NetSerializable]
public sealed class PortablePumpToggleMessage : BoundUserInterfaceMessage
{
    public bool NewStatus { get; }

    public PortablePumpToggleMessage(bool newStatus)
    {
        NewStatus = newStatus;
    }
}

[Serializable]
[NetSerializable]
public sealed class PortablePumpEjectTankMessage : BoundUserInterfaceMessage
{
    public PortablePumpEjectTankMessage()
    {
    }
}

[Serializable]
[NetSerializable]
public sealed class PortablePumpTogglePumpDirectionMessage : BoundUserInterfaceMessage
{
    public PortablePumpTogglePumpDirectionMessage()
    {
    }
}

[Serializable]
[NetSerializable]
public sealed class PortablePumpSetPumpPressureMessage : BoundUserInterfaceMessage
{
    public float Pressure;
    public PortablePumpSetPumpPressureMessage(float pressure)
    {
        Pressure = pressure;
    }
}
