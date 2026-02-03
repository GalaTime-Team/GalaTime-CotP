using Galatime;
using Galatime.Global;
using Galatime.UI.Helpers;
using Godot;

namespace Galatime.UI;

/// <summary> Represent the death screen in the game UI, which is shown when the player dies. </summary>
public partial class DeathScreenContainer : Control
{
    /// <summary> Default scene to load if no save data is available. </summary>
    private const string DefaultScenePath = "res://assets/scenes/Lobby.tscn";
    
    public HBoxContainer ChoiceButtonsContainer;
    public Button YesButton;
    public Button NoButton;
    public TypingLabel DeathText;
    public AnimationPlayer AnimationPlayer;

    public Tween Tween;

    public Tween GetTween()
    {
        Tween ??= GetTree().CreateTween().SetTrans(Tween.TransitionType.Cubic).SetParallel().SetPauseMode(Tween.TweenPauseMode.Process);
        return Tween;
    }

    public override void _Ready()
    {
        ChoiceButtonsContainer = GetNode<HBoxContainer>("ChoiceButtonsContainer");
        YesButton = GetNode<Button>("ChoiceButtonsContainer/YesButton");
        // NoButton = GetNode<Button>("ChoiceButtonsContainer/NoButton");
        DeathText = GetNode<TypingLabel>("DeathText");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        DeathText.OnTypingFinished += OnTypingFinished;
        YesButton.Pressed += OnYesPressed;
    }

    public void OnTypingFinished()
    {
        // Make it visible, so it can be interacted with.
        ChoiceButtonsContainer.Visible = true;

        GetTween();
        // Animate the choice buttons to fade in.
        Tween.TweenMethod(Callable.From<float>(x => ChoiceButtonsContainer.Modulate = new Color(1, 1, 1, x)), 0f, 1f, .5f).Finished += () => 
        {
            YesButton.GrabFocus();
        };
    }

    public void CallDeath()
    {
        WindowManager.Instance.ToggleWindow("death", false);
        GetTree().Paused = true;
        AnimationPlayer.Play("death");
    }

    public void OnYesPressed()
    {
        WindowManager.Instance.ToggleWindow("death", true);
        
        // Load the last save data to get the saved scene
        var saveData = GalatimeGlobals.LoadSave(PlayerVariables.CurrentSave);
        
        // Clear level objects to ensure a fresh start from the save
        LevelManager.Instance.LevelObjects.Clear();
        
        // Re-trigger save loading when the scene loads
        PlayerVariables.Instance.ShouldLoadSave = true;
        PlayerVariables.Instance.ShouldRestorePosition = true; // Restore position from save on death reload
        
        // Load the scene from the save data (or default to Lobby if save has no scene)
        var scenePath = !string.IsNullOrEmpty(saveData?.CurrentScene) 
            ? saveData.CurrentScene 
            : DefaultScenePath;
            
        GD.PrintRich($"[color=cyan]DEATH SCREEN[/color]: Reloading last save from scene: {scenePath}");
        GalatimeGlobals.Instance.LoadScene(scenePath);
    }
}
