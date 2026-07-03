using Content.Shared.Atmos.Piping.Unary.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Portable.Components;

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
    public bool Enabled { get; }
    public float Pressure { get; }
    public bool Connected { get; }
    public VentPumpDirection PumpDirection;
    public float TargetPressure;
    public string? TankLabel;
    public float TankPressure;

    public PortablePumpBoundUserInterfaceState(bool enabled, float pressure, bool connected, VentPumpDirection pumpDirection, float targetPressure, string? tankLabel, float tankPressure)
    {
        Enabled = enabled;
        Pressure = pressure;
        Connected = connected;
        PumpDirection = pumpDirection;
        TargetPressure = targetPressure;
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
