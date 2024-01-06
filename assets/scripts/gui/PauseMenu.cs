using Galatime.Global;
using Godot;

namespace Galatime.UI;

/// <summary> A node that handles all the pause in the game and has an interface. </summary>
public partial class PauseMenu : Control
{
    #region Nodes
    public ColorRect Background;
    public Panel Panel;
    public VBoxContainer ButtonsContainer;
    public AudioStreamPlayer MusicPlayer;

    // Windows
    public SettingsContainer SettingsContainer;

    // Buttons
    public Button ResumeButton;
    public Button SaveButton;
    public Button ReloadButton;
    public Button SettingsButton;
    public Button ExitButton;
    #endregion

    #region Properties
    private bool paused;

    /// <summary> Pauses the game using <see cref="Tree"/> and shows the menu when the game is paused. </summary>
    public bool Paused
    {
        get => paused;
        set
        {
            if (!WindowManager.Instance.ToggleWindow("pause", Paused, () => Paused = false, canOverlay: true)) return;
            // Don't pause a game when an animation is playing.
            if (Tw?.IsRunning() == true) return;
            if (CurrentWindow != null)
            {
                OpenWindow(CurrentWindow);
                return;
            }

            Visible = value;

            paused = value;
            GetTree().Paused = paused;
            // Set the value of a variable to check later if the animation is playing.
            Tw = GetTween();
            Tw.Finished += () =>
            {
                if (paused) GrabFirstControl(); // Focus on the first button when paused and when the animation ends.
                else GetViewport().GuiReleaseFocus();
            };
            // Create an animation that slides from the left corner and back again if there is no pause.
            Tw.TweenMethod(Callable.From<Vector2>(x => Panel.Position = x), Panel.Position, paused ? Vector2.Zero : new Vector2(-Panel.Size.X, 0), TransitionDuration);
            // Same thing, but with background transparency. Goes 0% transparent to 50% when paused.
            Tw.TweenMethod(Callable.From<float>(x => Background.Color = new Color(0, 0, 0, x)), Background.Color.A, paused ? 0.5f : 0, TransitionDuration);
            // Increasing volume of sound when paused.
            Tw.TweenMethod(Callable.From<float>(x => MusicPlayer.VolumeDb = x), MusicPlayer.VolumeDb, paused ? 0 : -80, TransitionDuration);
        }
    }
    #endregion

    #region Variables
    public bool WindowIsVisible;
    public Control CurrentWindow;

    public float TransitionDuration = 0.3f;
    public float MusicTransitionDuration = 3f;
    public Tween Tw;
    #endregion

    public override void _Ready()
    {
        #region Get nodes
        Background = GetNode<ColorRect>("Background");
        Panel = GetNode<Panel>("Panel");
        ButtonsContainer = GetNode<VBoxContainer>("Panel/ButtonsContainer");
        MusicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");

        // Windows
        SettingsContainer = GetNode<SettingsContainer>("SettingsContainer");

        // Buttons
        ResumeButton = ButtonsContainer.GetNode<Button>("ResumeButton");
        SaveButton = ButtonsContainer.GetNode<Button>("SaveButton");
        ReloadButton = ButtonsContainer.GetNode<Button>("ReloadButton");
        ExitButton = ButtonsContainer.GetNode<Button>("ExitButton");
        SettingsButton = ButtonsContainer.GetNode<Button>("SettingsButton");
        #endregion
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
        ResumeButton.Pressed += () => Paused = false;
        SaveButton.Pressed += () =>
        {
            Paused = false;
            globals.Save(PlayerVariables.CurrentSave, this);
        };
        ReloadButton.Pressed += () =>
        {
            Paused = false;
            LevelManager.Instance.ReloadLevel();
        };
        ExitButton.Pressed += () =>
        {
            Paused = false;
            globals.LoadScene();
            var levelManager = GetNode<LevelManager>("/root/LevelManager");
            levelManager.EndAudioCombat();
        };
        SettingsButton.Pressed += () => OpenWindow(SettingsContainer, SettingsContainer.FirstControl);
    }

    public void OpenWindow(Control window, Control focusTo = null)
    {
        if (Tw?.IsRunning() == true) return;

        WindowIsVisible = !WindowIsVisible;
        CurrentWindow = WindowIsVisible ? window : null;
        if (WindowIsVisible)
        {
            window.Visible = true;
            focusTo?.GrabFocus();
        }

        Tw = GetTween();
        Tw.TweenMethod(Callable.From<Vector2>(x => window.Position = x), window.Position, WindowIsVisible ? new Vector2(Panel.Size.X, 0) : new Vector2(-window.Size.X, 0), TransitionDuration * 2).Finished += () =>
        {
            if (!WindowIsVisible)
            {
                window.Visible = false;
                GrabFirstControl();
                if (window.Name == SettingsContainer.Name)
                {
                    var settingsGlobals = GetNode<SettingsGlobals>("/root/SettingsGlobals");
                    settingsGlobals.SaveSettings();
                }
            }
        };
    }

    public Tween GetTween() => CreateTween().SetTrans(Tween.TransitionType.Cubic).SetParallel();

    public void GrabFirstControl() => (ButtonsContainer.GetChild(0) as Control)?.GrabFocus();

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Paused = !Paused;
        }
    }
}
