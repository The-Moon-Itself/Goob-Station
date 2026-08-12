using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MassDriver;

/// <summary>
/// Used for the visualizer
/// </summary>
[Serializable, NetSerializable]
public enum MassDriverVisuals : byte
{
    Active,
    PanelOpen
}

