using System;
using System.Collections.Generic;
using System.Linq;
using Galatime.Interfaces;
using Godot;

namespace Galatime.Global;

public class CutsceneData
{
    public string ID;
    public Action Execute;

    public CutsceneData(string id, Action execute) => (ID, Execute) = (id, execute);
}

/// <summary> Represents a manager for cutscenes, which contains methods for cutscenes. </summary>
public partial class CutsceneManager : Node
{
    public GameLogger Logger { get; private set; } = new("CUTSCENES", GameLogger.ConsoleColor.Purple);
    public static CutsceneManager Instance { get; private set; }

    public Tween Tween { get; private set; }

    public string CurrentCutscene;
    /// <summary> List of cutscenes, registered in the game. Use <see cref="RegisterCutscene"/> to add new. </summary>
    public List<CutsceneData> Cutscenes { get; } = new();
    /// <summary> List of drama objects, registered in the game. Use <see cref="RegisterDramaObject"/> to add new. </summary>
    public List<IDrama> DramaObjects { get; } = new();

    public override void _Ready()
    {
        Instance = this;

        RegisterCutscene(
            new CutsceneData("test", () =>
            {
                var arthur = GetDramaObject("arthur");
                var raphael = GetDramaObject("raphael");
                var player = PlayerVariables.Instance.Player;

                if (Validate(arthur, raphael, player)) return;
                BlockPlayer();

                arthur.PlayDramaAnimation("WalkRight");
                raphael.PlayDramaAnimation("WalkRight");
                (raphael as TestCharacter).DisableHumanoidDoll = true;

                SetTween(parallel: true);
                GoToPosition(arthur as Node2D, new Vector2(1248, -240));
                GoToPosition(raphael as Node2D, new Vector2(1248, -336));
                TweenCallback(() =>
                {
                    arthur.PlayDramaAnimation("IdleRight");
                    raphael.PlayDramaAnimation("IdleRight");
                }).SetDelay(1f);
                TweenCallback(() =>
                {
                    raphael.PlayDramaAnimation("IdleDown");
                    player.StartDialog("test", () =>
                    {
                        arthur.PlayDramaAnimation("WalkRight");
                        raphael.PlayDramaAnimation("WalkRight");

                        SetTween(parallel: true);
                        GoToPosition(arthur as Node2D, new Vector2(1400, -240), 2f);
                        GoToPosition(raphael as Node2D, new Vector2(1400, -336), 2f);
                    });

                }).SetDelay(3f);
            }),
            new CutsceneData("test2", () => 
            {   
                var player = PlayerVariables.Instance.Player;
                if (Validate(player)) return;
                player.StartDialog("test2");
            })
        );
    }

    /// <summary> Move the given node to the specified position over a given duration. </summary>
    /// <param name="duration">In seconds</param>
    /// <param name="node">The node to move</param>
    public MethodTweener GoToPosition(Node2D node, Vector2 position, float duration = 1f)
    {
        return Tween.TweenMethod(Callable.From<Vector2>(v =>
        {
            if (IsInstanceValid(node)) node.GlobalPosition = v;
        }), node.GlobalPosition, position, duration);
    }

    public CallbackTweener TweenCallback(Action action) => Tween?.TweenCallback(Callable.From(action));

    /// <summary> Validates the given drama objects. </summary>
    /// <returns> True if all drama objects are valid, false otherwise. </returns>
    public bool Validate(params object[] dramaObjects)
    {
        // Check if any of the drama objects are null
        bool isNotValid = dramaObjects.All(drama => drama == null);

        if (isNotValid) Logger.Log("One or more drama objects are null or invalid. Cutscene will not be played.", GameLogger.LogType.Error);
        return isNotValid;
    }

    public override void _Process(double delta)
    {
        // Remove invalid drama objects.
        DramaObjects.ToList().ForEach(d => {
            if (!IsInstanceValid(d as Node2D) || d is not Node2D) DramaObjects.Remove(d);
        });
    }

    /// <summary> Sets the tween for the this <see cref="CutsceneManager"/> and returns it. </summary>
    public Tween SetTween(Tween.TransitionType type = Tween.TransitionType.Linear, bool parallel = false, Tween.EaseType ease = Tween.EaseType.InOut)
    {
        if (Tween != null)
        {
            Tween.Kill();
            Tween = null;
        }
        Tween = GetTree().CreateTween().SetTrans(type).SetParallel(parallel).SetEase(ease);

        return Tween;
    }

    /// <summary> Sets the current cutscene and executes it. </summary>
    /// <returns> True if the cutscene is found and executed, false otherwise. </returns>
    public bool StartCutscene(string cutscene)
    {
        CurrentCutscene = cutscene;

        // Find the cutscene with the given ID.
        var ct = Cutscenes.Find(c => c.ID == cutscene);

        // If the cutscene is not found, print an error message and return false.
        if (ct == null)
        {
            Logger.Log($"Cutscene {cutscene} not found.", GameLogger.LogType.Error);
            return false;
        }
        ct.Execute?.Invoke();

        // Return true to indicate that the cutscene was found and executed.
        return true;
    }

    public void RegisterCutscene(params CutsceneData[] cutscenes)
    {
        foreach (var c in cutscenes) Cutscenes.Add(c);
    }

    public void RegisterDramaObject(Node2D dramaObject)
    {
        var dr = dramaObject as IDrama;
        if (dr == null)
        {
            Logger.Log($"Drama object {dramaObject.Name} is not a drama object. Please, implement the IDrama interface.", GameLogger.LogType.Error);
            return;
        }
        DramaObjects.Add(dr);
        Logger.Log($"Registered drama object: {dr.DramaID}", GameLogger.LogType.Success);
        Logger.Log(string.Join(", ", DramaObjects.Select(c => c.DramaID)));
    }

    public IDrama GetDramaObject(string id)
    {
        var dro = DramaObjects.Find(d => d.DramaID == id);
        if (dro == null)
        {
            Logger.Log($"Drama object {id} not found.", GameLogger.LogType.Error);
            return null;
        }

        return dro;
    }

    public void BlockPlayer(bool block = true)
    {
        var player = PlayerVariables.Instance.Player;
        player.IsPlayerFrozen = block;
    }
}
