
using Robust.Client.Animations;

namespace Content.Goobstation.Client.MassDriver;

[RegisterComponent]
public sealed partial class MassDriverVisualsComponent : Component
{

    /// <summary>
    /// The state while the mass driver is idle
    /// </summary>
    [DataField("idleState", required: true)]
    public string IdleState = default!;

    /// <summary>
    /// The state while the mass driver is actively launching. Makes up LaunchAnimation.
    /// </summary>
    [DataField("activeState", required: true)]
    public string ActiveState = default!;

    /// <summary>
    /// How long the launch animation is.
    /// </summary>
    [DataField]
    public float LaunchTime = 0.5f;

    /// <summary>
    /// The animation to be played when the mass driver is activated
    /// </summary>
    public object LaunchAnimation = default!;


    /// <summary>
    /// An ID number for a request from the server to play an animation.
    /// The launch animation will only play when a request has a different number to the one stored here.
    /// </summary>
    public int animationNumber = 0;

    /// <summary>
    /// The key used to index the animation played when activating the mass driver.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public const string AnimationKey = "mass_driver_animation";
}
