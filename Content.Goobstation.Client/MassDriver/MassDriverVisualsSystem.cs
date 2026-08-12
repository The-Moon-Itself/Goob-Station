
using Content.Goobstation.Shared.MassDriver;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.MassDriver;

public sealed class PortablePumpSystem : VisualizerSystem<MassDriverVisualsComponent>
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MassDriverVisualsComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<MassDriverVisualsComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnComponentInit(Entity<MassDriverVisualsComponent> ent, ref ComponentInit args)
    {
        ent.Comp.LaunchAnimation = new Animation
        {
            Length = TimeSpan.FromSeconds(ent.Comp.LaunchTime),
            AnimationTracks = {
                new AnimationTrackSpriteFlick() {
                    LayerKey = MassDriverVisualLayers.Base,
                    KeyFrames = {new AnimationTrackSpriteFlick.KeyFrame(ent.Comp.ActiveState, 0f)}
                },
            }
        };
    }

    private void OnAnimationCompleted(Entity<MassDriverVisualsComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != MassDriverVisualsComponent.AnimationKey)
            return;

        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!TryComp<AppearanceComponent>(ent, out var appearance))
            return;

        UpdateApperance(ent, sprite, appearance, false);
    }

    protected override void OnAppearanceChange(EntityUid uid, MassDriverVisualsComponent component, ref AppearanceChangeEvent args)
    {
        Entity<MassDriverVisualsComponent> ent = (uid, component);
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<bool>(ent, MassDriverVisuals.Active, out var active, args.Component))
            active = false;

        UpdateApperance(ent, args.Sprite, args.Component, active);
    }

    private void UpdateApperance(Entity<MassDriverVisualsComponent> ent, SpriteComponent sprite, AppearanceComponent appearance, bool active, AnimationPlayerComponent? animPlayer = null)
    {
        if (!Resolve(ent, ref animPlayer))
            return;

        if (_animation.HasRunningAnimation(ent, animPlayer, MassDriverVisualsComponent.AnimationKey))
            return;

        if (!AppearanceSystem.TryGetData<int>(ent, MassDriverVisuals.Active, out var animNumber, appearance))
            animNumber = ent.Comp.animationNumber;

        if (animNumber != ent.Comp.animationNumber)
        {
            _animation.Play((ent, animPlayer), (Animation) ent.Comp.LaunchAnimation, MassDriverVisualsComponent.AnimationKey);
            ent.Comp.animationNumber = animNumber;
        }
        else
            _sprite.LayerSetRsiState((ent.Owner, sprite), MassDriverVisualLayers.Base, ent.Comp.IdleState);
    }
}

public enum MassDriverVisualLayers : byte
{
    Base,
    Wires
}
