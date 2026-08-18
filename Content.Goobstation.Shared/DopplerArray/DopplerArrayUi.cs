
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.DopplerArray;

[Serializable, NetSerializable]
public enum DopplerArrayUIKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class DopplerArrayUIState : BoundUserInterfaceState
{
    public List<TachyonRecord> Records;

    public bool ResearchEnabled;

    public DopplerArrayUIState(List<TachyonRecord> records, bool researchEnabled)
    {
        Records = records;
        ResearchEnabled = researchEnabled;
    }
}

/// <summary>
/// Deletes a record in the doppler array by index
/// </summary>
[Serializable, NetSerializable]
public sealed class DopplerArrayDeleteRecord : BoundUserInterfaceMessage
{
    public readonly uint Index;

    public DopplerArrayDeleteRecord(uint index)
    {
        Index = index;
    }
}
