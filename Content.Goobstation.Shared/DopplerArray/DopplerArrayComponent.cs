
using Content.Goobstation.Shared.DopplerArray;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Research.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.DopplerArray;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class DopplerArrayComponent : Component
{
    /// <summary>
    /// How far the doppler array can detect explosions.
    /// </summary>
    [DataField]
    public float MaxDistance = 150f;

    /// <summary>
    /// How many seconds after sensing an explosion until the array is ready to sense the next explosion.
    /// </summary>
    [DataField]
    public float Cooldown = 5f;

    /// <summary>
    /// Timestamp for when the array is off cooldown to detect another explosion.
    /// </summary>
    public TimeSpan NextSense = TimeSpan.Zero;

    /// <summary>
    /// How long to wait between each message spoken
    /// </summary>
    [DataField]
    public float AnnouceCooldown = 0.1f;

    /// <summary>
    /// Messages to say
    /// </summary>
    public Queue<String> MessageBuffer = new();

    /// <summary>
    /// The number to be given to the name of the next record.
    /// </summary>
    public int RecordNumber = 1;

    [DataField, AutoNetworkedField]
    public List<TachyonRecord> Records = new();

    /// <summary>
    /// Whether the doppler array is capable of generating research points
    /// This requires a Research Client Component on the entity to funciton.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ResearchEnabled = false;

    /// <summary>
    /// The maximum amount of research points the array can award.
    /// Larger bombs approach this asymptotically.
    /// </summary>
    [DataField]
    public float MaxResearchPayout = 30000;

    /// <summary>
    /// The explosion range needed to payout 90% of the max research point payout.
    /// </summary>
    [DataField]
    public float ResearchRiseTime = 50;

    /// <summary>
    /// Whether the doppler array is capable of selling explosion information.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ProfitEnabled = false;

    /// <summary>
    /// Which account to payout profit to
    /// </summary>
    [DataField]
    public ProtoId<CargoAccountPrototype> LinkedAccount = "Science";

    /// <summary>
    /// How many credits the science department should be rewarded per research point gained.
    /// </summary>
    [DataField]
    public float ProfitMultiplier = 1;

    /// <summary>
    /// The minimum radius required to give a payout
    /// </summary>
    [DataField]
    public float PayoutRequiredRadius = 8;
}
