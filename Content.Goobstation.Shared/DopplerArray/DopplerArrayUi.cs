

using Content.Goobstation.Shared.DopplerArray;
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
    public List<TachyonRecord>? Records;

    public DopplerArrayUIState(List<TachyonRecord> records)
    {
        Records = records;
    }
}
