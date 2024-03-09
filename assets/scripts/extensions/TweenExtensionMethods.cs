using System;
using Godot;

namespace ExtensionMethods;

public static class TweenExtension
{
    /// <summary> Shorthand for tweening a method. </summary>
    public static MethodTweener MTween<[MustBeVariant] T>(this Tween tween, Action<T> action, Variant from, Variant to, float duration) =>
        tween.TweenMethod(Callable.From<T>(action), from, to, duration);

    /// <summary> Shorthand for tweening a callback. </summary>
    public static CallbackTweener CTween(this Tween tween, Action callback) =>
        tween.TweenCallback(Callable.From(callback));
}