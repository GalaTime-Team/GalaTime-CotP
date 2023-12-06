using Galatime.Global;
using Godot;
using System;
using System.Collections.Generic;

namespace Galatime.UI;

public enum SwipeDirection { UP, RIGHT, DOWN, LEFT, BACK }

public sealed class MainMenuPage
{
    public static Vector2 GetOffset(SwipeDirection swipeDirection, Vector2 size)
    {
        var offset = new Dictionary<SwipeDirection, Vector2>()
        {
            {SwipeDirection.UP, new Vector2(0, size.Y)},
            {SwipeDirection.RIGHT, new Vector2(-size.X, 0)},
            {SwipeDirection.DOWN, new Vector2(0, -size.Y)},
            {SwipeDirection.LEFT, new Vector2(size.X, 0)}
        };
        return offset[swipeDirection];
    }

    private string scenePath;
    public string ScenePath
    {
        get => scenePath;
        set
        {
            scenePath = value;
            Instance ??= GD.Load<PackedScene>(value).Instantiate<Control>();
        }
    }
    public SwipeDirection SwipeDirection;
    public Control Instance;
    public MainMenuPage(SwipeDirection swipeDirection, string scenePath) =>
        (SwipeDirection, ScenePath) = (swipeDirection, scenePath);
}

public partial class MainMenu : Control
{
    /// <summary> The time to transition to the each page. </summary>
    public float TransitionTime = 2f;
    /// <summary> If the current page is the main menu. </summary>
    private bool IsMainMenu = true;

    /// <summary> The buttons of the main menu. </summary>
    private Godot.Collections.Array<Node> MenuButtons;
    /// <summary> The buttons, which are has visuals. </summary>
    private Godot.Collections.Array<Node> VisualButtons;

    /// <summary> The current focus element. </summary>
    private Control CurrentFocus;
    /// <summary> The control that had focus before a popup was opened. </summary>
    private Control BeforePopupFocus;
    /// <summary> The current control representing the current page. </summary>
    private Control CurrentPageControl;
    /// <summary> The current direction of the swipe. </summary>
    private SwipeDirection CurrentSwipeDirection;

    #region Audio
    private AudioStreamPlayer AudioButtonHover;
    private AudioStreamPlayer AudioButtonAccept;
    private AudioStreamPlayer AudioMenu;
    private AudioStreamPlayer AudioMenuMuffled;
    private AudioStreamPlayer AudioMenuWhoosh;
    #endregion

    private AnimationPlayer AnimationPlayer;

    private Control StartMenuControl;
    private Control MainMenuControl;
    private Control SettingsMenuControl;
    private Control CreditsMenuControl;

    private SettingsContainer SettingsContainer;

    /// <summary> The dictionaries of all menus (pages) of the main menu. </summary>
    private Dictionary<string, Control> Menus = new();

    private Label VersionLabel;
    private Control AcceptContainer;
    private PackedScene SaveContainerScene;

    private Label AcceptName;
    private LabelButton AcceptYesButton;
    private LabelButton AcceptNoButton;

    private LabelButton ViewSavesButton;
    private VBoxContainer MainMenuControlButtons;

    private Timer DelayInteract;

    public delegate void OnAccept(bool result);
    public static OnAccept onAccept;
    public Action whenPressedYes = () => onAccept(true);
    public Action whenPressedNo = () => onAccept(false);

    public override void _Ready()
    {
        ParseCMDLineArgs();

        #region Get nodes
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        DelayInteract = new Timer
        {
            OneShot = true,
            WaitTime = TransitionTime
        };
        AddChild(DelayInteract);

        AudioButtonHover = GetNode<AudioStreamPlayer>("AudioStreamPlayerButtonHover");
        AudioButtonAccept = GetNode<AudioStreamPlayer>("AudioStreamPlayerButtonAccept");
        AudioMenu = GetNode<AudioStreamPlayer>("AudioStreamPlayerMenu");
        AudioMenuMuffled = GetNode<AudioStreamPlayer>("AudioStreamPlayerMenuMuffled");
        AudioMenuWhoosh = GetNode<AudioStreamPlayer>("AudioStreamPlayerWhoosh");

        StartMenuControl = GetNode<Control>("StartMenuContainer");
        MainMenuControl = GetNode<Control>("MainMenuContainer");
        SettingsMenuControl = GetNode<Control>("SettingsMenuContainer");
        CreditsMenuControl = GetNode<Control>("CreditsContainer");

        SettingsContainer = GetNode<SettingsContainer>("SettingsMenuContainer/Settings");

        Menus.Add("start", StartMenuControl);
        Menus.Add("main_menu", MainMenuControl);
        Menus.Add("settings", SettingsMenuControl);
        Menus.Add("credits", CreditsMenuControl);

        MainMenuControlButtons = GetNode<VBoxContainer>("MainMenuContainer/VBoxContainer");
        VersionLabel = GetNode<Label>("VersionLabel");

        AcceptContainer = GetNode<Control>("AcceptContainer");
        AcceptYesButton = GetNode<LabelButton>("AcceptContainer/VBoxContainer/HBoxContainer/Yes");
        AcceptNoButton = GetNode<LabelButton>("AcceptContainer/VBoxContainer/HBoxContainer/No");
        AcceptYesButton.PivotOffset = new Vector2(10, 6);
        AcceptNoButton.PivotOffset = new Vector2(7, 6);
        AcceptName = GetNode<Label>("AcceptContainer/VBoxContainer/Name");
        #endregion

        StartOpacityTransition();
        InitializeSavesContainers();
        InitializeMainMenuButtons();
        UpdateSaves();

        GetViewport().GuiFocusChanged += guiFocusChanged;
        GalatimeGlobals.CheckSaves();

        VersionLabel.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.version}\n{GalatimeConstants.versionDescription}";
        GetTree().Root.Title = "GalaTime - Main Menu";
    }

    private void InitializeMainMenuButtons()
    {
        MenuButtons = GetNode("MainMenuContainer/VBoxContainer").GetChildren();
        for (int i = 0; i < MenuButtons.Count; i++)
        {
            if (MenuButtons[i] is not LabelButton b) continue;
            if (i == 0) b.GrabFocus();
            var page = (string)b.GetMeta("page");
            b.Pressed += () => MainMenuButtonsPressed(page);
        }
    }

    private void StartOpacityTransition() => GetTween().TweenProperty(MainMenuControl, "modulate", new Color(1f, 1f, 1f), TransitionTime);
    private void InitializeSavesContainers()
    {
        SaveContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/SaveContainer.tscn");
        ViewSavesButton = GetNode<LabelButton>("StartMenuContainer/ViewSavesFolder");
        ViewSavesButton.Pressed += () => OS.ShellOpen(ProjectSettings.GlobalizePath(GalatimeConstants.savesPath));
    }

    public void ParseCMDLineArgs()
    {
        if (GalatimeGlobals.CMDArgs.ContainsKey("save"))
        {
            PlayerVariables.currentSave = int.Parse(GalatimeGlobals.CMDArgs["save"]);

            var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
            globals.LoadScene("res://assets/scenes/Lobby.tscn");

            return;
        }
    }

    public void guiFocusChanged(Control control)
    {
        if (control != null) CurrentFocus = control;
    }

    public bool AcceptIsVisible() => AcceptContainer.Modulate.A != 0;

    /// <summary>
    /// Displays an accept dialog with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for the dialog.</param>
    /// <param name="callback">The callback function to call when the dialog is accepted or dismissed.</param>
    /// <param name="invertedColors">Whether to use inverted colors for the dialog.</param>

    // TODO: Make accept dialog with another soultion of the Godot API.
    public void appearAccept(string reason, OnAccept callback, bool invertedColors = false)
    {
        if (invertedColors)
        {
            AcceptYesButton.SetMeta("ColorHoverOverride", new Color(1f, 0f, 0f));
            AcceptNoButton.SetMeta("ColorHoverOverride", new Color(1, 1, 0));
        }
        else
        {
            AcceptYesButton.SetMeta("ColorHoverOverride", new Color(1, 1, 0));
            AcceptNoButton.SetMeta("ColorHoverOverride", new Color(1f, 0f, 0f));
        }


        BeforePopupFocus = CurrentFocus;
        AcceptNoButton.GrabFocus();

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        AcceptName.Text = reason;

        tween.TweenProperty(AcceptContainer, "modulate", new Color(1, 1, 1, 1), 0.3f);

        AcceptContainer.MouseFilter = MouseFilterEnum.Stop;
        AcceptYesButton.MouseFilter = MouseFilterEnum.Stop;
        AcceptNoButton.MouseFilter = MouseFilterEnum.Stop;

        AcceptYesButton.Pressed -= whenPressedYes;
        AcceptNoButton.Pressed -= whenPressedNo;

        AcceptYesButton.Pressed += whenPressedYes;
        AcceptNoButton.Pressed += whenPressedNo;

        onAccept = callback;
    }

    /// <summary>
    /// Hides the accept dialog.
    /// </summary>
    public void disappearAccept()
    {
        BeforePopupFocus.GrabFocus();

        AcceptYesButton.ReleaseFocus();
        AcceptNoButton.ReleaseFocus();

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        tween.TweenProperty(AcceptContainer, "modulate", new Color(1, 1, 1, 0), 0.3f);

        AcceptContainer.MouseFilter = MouseFilterEnum.Ignore;
        AcceptYesButton.MouseFilter = MouseFilterEnum.Ignore;
        AcceptNoButton.MouseFilter = MouseFilterEnum.Ignore;

        onAccept = null;
    }

    public void UpdateSaves()
    {
        var saves = GalatimeGlobals.getSaves();
        GD.PrintRich("[color=purple]MAIN MENU[/color]: [color=cyan]UPDATE SAVES[/color]");
        var savesContainers = GetNode("StartMenuContainer/SavesContainer").GetChildren();

        for (int i = 0; i < savesContainers.Count; i++)
        {
            var item = savesContainers[i];
            VisualButtons.Remove(item);
            item.QueueFree();
        }
        for (int i = 0; i < saves.Count; i++)
        {
            var instance = SaveContainerScene.Instantiate<SaveContainer>();
            GetNode("StartMenuContainer/SavesContainer").AddChild(instance);

            var deleteButton = instance.GetDeleteButtonInstance();
            var playButton = instance.GetPlayButtonInstance();

            ViewSavesButton.FocusNeighborTop = playButton.GetPath();
            ViewSavesButton.FocusNeighborRight = playButton.GetPath();
            ViewSavesButton.FocusNeighborLeft = playButton.GetPath();
            ViewSavesButton.FocusNeighborBottom = playButton.GetPath();
            ViewSavesButton.FocusNext = playButton.GetPath();
            ViewSavesButton.FocusPrevious = playButton.GetPath();

            var save = saves[i];
            deleteButton.Pressed += () => DeleteSaveButtonInput((int)save["id"]);
            playButton.Pressed += () => PlayButtonPressed(instance.id);

            instance.LoadData(saves[i]);
        }
    }

    public void PlayButtonPressed(int id)
    {
        AnimationPlayer.Play("start");
        AudioButtonAccept.Play();

        GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Selected save {id}, waiting for end of the animation[/color]");
        PlayerVariables.currentSave = id;

        DelayInteract.Start();
    }

    public void OnStartAnimationEnded()
    {
        var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
        globals.LoadScene("res://assets/scenes/Lobby.tscn");
    }

    public void DeleteSaveButtonInput(int saveId)
    {
        appearAccept("Do you really want to delete the save?", (bool result) =>
        {
            if (result)
            {
                GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Deleting save {saveId}[/color]");
                GalatimeGlobals.createBlankSave(saveId);
                UpdateSaves();
                disappearAccept();
            }
            else
            {
                disappearAccept();
            }
        }
        , true);
    }

    /// <summary> Handles event of main menu buttons being pressed </summary>
    public void MainMenuButtonsPressed(string page)
    {
        SwitchPage(page);
    }

    public Tween GetTween() => GetTree().CreateTween().SetTrans(Tween.TransitionType.Cubic).SetParallel(true);

    /// <summary> Switches to the specified page. </summary>
    /// <param name="page"> The name of the page to switch to. </param>
    public void SwitchPage(string page)
    {
        if (DelayInteract.TimeLeft > 0) return;
        DelayInteract.Start();

        IsMainMenu = false;
        GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Switch to {page} menu[/color]");
        AudioMenuWhoosh.Play();

        var currentPage = Menus[page];

        // A dictionary to store the swipe direction and the focus button for each page
        var pageData = new Dictionary<string, (SwipeDirection, Control)>()
        {
            {"start", (SwipeDirection.UP, ViewSavesButton)},
            {"settings", (SwipeDirection.LEFT, SettingsContainer.FirstControl)},
            {"credits", (SwipeDirection.DOWN, null)}
        };

        // Get the swipe direction and the focus button for the page
        var (direction, focusButton) = pageData[page];

        DisableMenus();
        GetTree().CreateTimer(TransitionTime).Timeout += () =>
        {
            DisableMenuButtons(true);
            focusButton?.GrabFocus();
        };

        // Swipe the page
        SwipePage(direction, MainMenuControl, currentPage);

        // Set the focus button if not null
        if (focusButton != null) GetTree().CreateTimer(TransitionTime).Timeout += () => focusButton.GrabFocus();

        // Adjust the audio volume for the settings page
        if (page == "settings")
        {
            var linearTween = GetTree().CreateTween();
            linearTween.SetParallel(true);

            linearTween.TweenProperty(AudioMenuMuffled, "volume_db", 0, TransitionTime / 2);
            linearTween.TweenProperty(AudioMenu, "volume_db", -80, TransitionTime);
        }

        // Make the page visible
        currentPage.Visible = true;
    }

    /// <summary> Gets the opposite swipe direction. </summary>
    private static SwipeDirection GetOppositeSwipeDirection(SwipeDirection direction) => direction switch
    {
        SwipeDirection.UP => SwipeDirection.DOWN,
        SwipeDirection.RIGHT => SwipeDirection.LEFT,
        SwipeDirection.DOWN => SwipeDirection.UP,
        SwipeDirection.LEFT => SwipeDirection.RIGHT,
        _ => SwipeDirection.UP,
    };

    /// <summary>
    /// Swipes the current page to the specified direction.
    /// </summary>
    /// <param name="direction">The direction to swipe the page.</param>
    /// <param name="previousControl">The control that is currently being displayed.</param>
    /// <param name="nextControl">The control that will be displayed after the swipe.</param>
    private void SwipePage(SwipeDirection direction, Control previousControl, Control nextControl)
    {
        var previousPosition = previousControl.Position;

        // A dictionary to store the position offsets for each direction.
        var offset = new Dictionary<SwipeDirection, Vector2>()
        {
            {SwipeDirection.UP, new Vector2(0, MainMenuControl.Size.Y)},
            {SwipeDirection.RIGHT, new Vector2(-MainMenuControl.Size.X, 0)},
            {SwipeDirection.DOWN, new Vector2(0, -MainMenuControl.Size.Y)},
            {SwipeDirection.LEFT, new Vector2(MainMenuControl.Size.X, 0)}
        };

        // Add the offset to the previous position
        previousPosition += offset[direction];

        // Animate the previous control to the invisible position.
        GetTween().TweenProperty(previousControl, "position", previousPosition, TransitionTime);
        previousPosition.X += MainMenuControl.Size.X;

        // Animate the next control to the next position.
        GetTween().TweenProperty(nextControl, "position", Vector2.Zero, TransitionTime);

        CurrentPageControl = nextControl;
        CurrentSwipeDirection = direction;
    }


    public void ToMainMenu()
    {
        GD.PrintRich($"[color=purple]MAIN MENU[/color]: Condition checking: [color=cyan]isMainMenu? - {IsMainMenu}, acceptIsVisible? - {AcceptIsVisible()}[/color]");
        if (DelayInteract.TimeLeft > 0) return;
        if (AcceptIsVisible()) return;
        if (IsMainMenu && !AcceptIsVisible())
        {
            appearAccept("Are you sure do you want to quit a game?", (bool result) =>
                {
                    if (result) GetTree().Quit();
                    else disappearAccept();
                }, true);
            IsMainMenu = true;
            return;
        }
        if (IsMainMenu && AcceptIsVisible())
        {
            disappearAccept();
            return;
        };

        DisableMenuButtons(false);

        DelayInteract.Start();
        AudioMenuWhoosh.Play();

        var linearTween = GetTree().CreateTween();
        linearTween.SetParallel(true);

        SwipePage(GetOppositeSwipeDirection(CurrentSwipeDirection), CurrentPageControl, MainMenuControl);

        linearTween.TweenProperty(AudioMenuMuffled, "volume_db", -80, TransitionTime);
        linearTween.TweenProperty(AudioMenu, "volume_db", 0, TransitionTime / 2);

        for (int i = 0; i < MenuButtons.Count; i++)
        {
            var button = MenuButtons[i] as Control;
            if (i == 0) button.GrabFocus();
        }

        GetTree().CreateTimer(TransitionTime).Timeout += () => DisableMenus();

        // Save settings to file when back to main menu.
        GetNode<SettingsGlobals>("/root/SettingsGlobals").SaveSettings();

        IsMainMenu = true;
    }

    void DisableMenus()
    {
        foreach (var menu in Menus)
        {
            if (menu.Key == "main_menu") continue;
            menu.Value.Visible = false;
        }
    }

    void DisableMenuButtons(bool yes)
    {
        foreach (var item in MainMenuControlButtons.GetChildren())
        {
            if (item is LabelButton button) button.Visible = !yes;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel")) ToMainMenu();
    }
}
