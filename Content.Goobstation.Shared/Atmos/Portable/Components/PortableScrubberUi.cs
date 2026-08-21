using Content.Shared.Atmos;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Atmos.Portable;

[Serializable]
[NetSerializable]
public enum PortableScrubberUiKey
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class PortableScrubberToggleMessage : BoundUserInterfaceMessage;

[Serializable]
[NetSerializable]
public sealed class PortableScrubberFilterGasToggleMessage : BoundUserInterfaceMessage
{
    public Gas ToggledGas { get; }
    public PortableScrubberFilterGasToggleMessage(Gas toggledGas)
    {
        ToggledGas = toggledGas;
    }
}

[Serializable]
[NetSerializable]
public sealed class PortableScrubberEjectTankMessage : BoundUserInterfaceMessage;

[Serializable]
[NetSerializable]
public sealed class PortableScrubberBoundUserInterfaceState : BoundUserInterfaceState
{
    public float Pressure { get; }
    public bool IsFull { get; }
    public bool Connected { get; }
    public string? TankLabel;
    public float TankPressure;

    public PortableScrubberBoundUserInterfaceState(float pressure, bool isFull, bool connected, string? tankLabel, float tankPressure)
    {
        Pressure = pressure;
        IsFull = isFull;
        Connected = connected;
        TankLabel = tankLabel;
        TankPressure = tankPressure;
    }
}
