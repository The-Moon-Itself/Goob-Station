using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Portable.Components;

[Serializable]
[NetSerializable]
public enum PortableScrubberUiKey
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class PortableScrubberToggleMessage : BoundUserInterfaceMessage
{
    public bool NewStatus { get; }

    public PortableScrubberToggleMessage(bool newStatus)
    {
        NewStatus = newStatus;
    }
}

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
public sealed class PortableScrubberEjectTankMessage : BoundUserInterfaceMessage
{
    public PortableScrubberEjectTankMessage()
    {
    }
}

[Serializable]
[NetSerializable]
public sealed class PortableScrubberBoundUserInterfaceStatusState : BoundUserInterfaceState
{
    public bool Enabled { get; }
    public float Pressure { get; }
    public bool IsFull { get; }
    public bool Connected { get; }
    public HashSet<Gas> FilterGases;
    public string? TankLabel;
    public float TankPressure;

    public PortableScrubberBoundUserInterfaceStatusState(bool enabled, float pressure, bool isFull, bool connected, HashSet<Gas> filterGases, string? tankLabel, float tankPressure)
    {
        Enabled = enabled;
        Pressure = pressure;
        IsFull = isFull;
        Connected = connected;
        FilterGases = new HashSet<Gas>(filterGases);
        TankLabel = tankLabel;
        TankPressure = tankPressure;
    }
}
