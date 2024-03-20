using Galatime.Global;
using Galatime.UI.Helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary> The previous page of the main menu. </summary>
    private string PreviousPage;
    /// <summary> The current page of the main menu. </summary>
    private string CurrentPage;
    /// <summary> The current focus element. </summary>
    private Control CurrentFocus;
    /// <summary> The control that had focus before a popup was opened. </summary>
    private Control BeforePopupFocus;
    /// <summary> The current control representing the current page. </summary>
    private Control CurrentPageControl;
    /// <summary> The current direction of the swipe. </summary>
    private SwipeDirection CurrentSwipeDirection;

    public AcceptWindow AcceptWindow;

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
    private PackedScene SaveContainerScene;

    private LabelButton ViewSavesButton;
    private VBoxContainer MainMenuControlButtons;

    private Timer DelayInteract;

    public override void _Ready()
    {
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

        Callable.From(() => AcceptWindow ??= AcceptWindow.CreateInstance()).CallDeferred();
        #endregion

        StartOpacityTransition();
        InitializeSavesContainers();
        InitializeMainMenuButtons();
        UpdateSaves();

        // Initialize back buttons to back to the main menu.
        // Since GetNodesInGroup returns an Godot.Collections.Array, it a little bit junky, but cast it to a regular array to optimize calls, because Godot arrays marshalling is slow.
        var backButtons = GetTree().GetNodesInGroup("exit_button").Cast<LabelButton>().ToArray(); 
        Array.ForEach(backButtons, x => x.Pressed += ToMainMenu);

        GalatimeGlobals.CheckSaves();
        VersionLabel.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.Version}\n{GalatimeConstants.VersionDescription}";

        MusicManager.Instance.Play("galatime");
    }

    private void InitializeMainMenuButtons()
    {
        MenuButtons = GetNode("MainMenuContainer/VBoxContainer").GetChildren();
        for (int i = 0; i < MenuButtons.Count; i++)
        {
            if (MenuButtons[i] is not LabelButton b) continue;
            if (i == 0) b.GrabFocus();
            if (i != 3) // Skip the "Exit" button
            {
                var page = (string)b.GetMeta("page");
                b.Pressed += () => SwitchPage(page);
            }
            else
            {
                if (i == 3) b.Pressed += () => CallExitAccept();
            }
        }
    }

    private void StartOpacityTransition() => GetTween().TweenProperty(MainMenuControl, "modulate", new Color(1f, 1f, 1f), TransitionTime);
    private void InitializeSavesContainers()
    {
        SaveContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/SaveContainer.tscn");
        ViewSavesButton = GetNode<LabelButton>("StartMenuContainer/ViewSavesFolder");
        ViewSavesButton.Pressed += () => OS.ShellOpen(ProjectSettings.GlobalizePath(GalatimeConstants.SavesPath));
    }

    public void UpdateSaves()
    {
        var saves = GalatimeGlobals.GetSaves();
        GD.PrintRich("[color=purple]MAIN MENU[/color]: [color=cyan]UPDATE SAVES[/color]");
        var savesContainers = GetNode("StartMenuContainer/SavesContainer").GetChildren();

        for (int i = 0; i < savesContainers.Count; i++)
        {
            var item = savesContainers[i];
            item.QueueFree();
        }
        for (int i = 0; i < GalatimeGlobals.MaxSaves; i++)
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

            deleteButton.Pressed += () => DeleteSaveButtonInput(deleteButton);

            instance.id = i + 1;
            if (i < saves.Count) instance.LoadData(saves[i]);
            else instance.LoadData(new());

            playButton.Pressed += () => PlayButtonPressed(instance.id);
        }
    }

    public void PlayButtonPressed(int id)
    {
        AnimationPlayer.Play("start");

        GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Selected save {id}, waiting for end of the animation[/color]");
        PlayerVariables.Instance.SetSave(id - 1);

        DelayInteract.Start();
    }

    public void OnStartAnimationEnded()
    {
        var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
        globals.LoadScene("res://assets/scenes/Lobby.tscn");
    }

    public void DeleteSaveButtonInput(Control button)
    {
        AcceptWindow.CallAccept((bool result) =>
        {
            if (result) UpdateSaves();
            // TODO: Implement delete
        }, "Do you really want to delete the save?", AcceptWindow.AcceptType.YesNo, true, button);
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

        PreviousPage = CurrentPage;
        CurrentPage = page;
    
        var currentPage = Menus[page];

        // A dictionary to store the swipe direction and the focus button for each page
        var pageData = new Dictionary<string, (SwipeDirection, Control)>()
        {
            {"start", (SwipeDirection.UP, ViewSavesButton)},
            {"settings", (SwipeDirection.LEFT, SettingsContainer.FirstControl)},
            {"credits", (SwipeDirection.DOWN, GetNode<Control>("CreditsContainer/BackButton"))} // Yes, it's a bit hacky
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
        if (CurrentPage == "settings")
        {
            MusicManager.Instance.SwitchAudio(false, TransitionTime);
        }

        // Make the page visible
        currentPage.Visible = true;
    }

    /// <summary> Gets the opposite swipe direction. </summary>
    private static SwipeDirection GetOppositeSwipeDirection(SwipeDirection direction)
        => (SwipeDirection)(((int)direction + 2) % 4);

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

        nextControl.Position = offset[GetOppositeSwipeDirection(direction)];
        // Animate the previous control to the invisible position.
        GetTween().TweenProperty(previousControl, "position", previousPosition, TransitionTime);
        previousPosition.X += MainMenuControl.Size.X;

        // Animate the next control to the next position.
        GetTween().TweenProperty(nextControl, "position", Vector2.Zero, TransitionTime);

        CurrentPageControl = nextControl;
        CurrentSwipeDirection = direction;
    }


    public void ToMainMenu() // It's hacky, I don't want to rewrite it
    {
        if (DelayInteract.TimeLeft > 0) return;
        if (IsMainMenu && !AcceptWindow.Shown)
        {
            CallExitAccept();
            IsMainMenu = true;
            return;
        }
        if (IsMainMenu && AcceptWindow.Shown)
        {
            AcceptWindow.Shown = false;
            return;
        };

        PreviousPage = CurrentPage;
        CurrentPage = "main_menu";

        AcceptWindow.Shown = false;

        DisableMenuButtons(false);

        DelayInteract.Start();
        AudioMenuWhoosh.Play();

        SwipePage(GetOppositeSwipeDirection(CurrentSwipeDirection), CurrentPageControl, MainMenuControl);

        MusicManager.Instance.SwitchAudio(true, TransitionTime);

        for (int i = 0; i < MenuButtons.Count; i++)
        {
            var button = MenuButtons[i] as Control;
            if (i == 0) button.GrabFocus();
        }

        GetTree().CreateTimer(TransitionTime).Timeout += () => DisableMenus();

        // Save settings to file when back to main menu.
        if (PreviousPage == "settings") SettingsGlobals.Instance.SaveSettings();

        IsMainMenu = true;
    }

    void CallExitAccept() => AcceptWindow.CallAccept((bool result) =>
    {
        if (result) GetTree().Quit();
    }, "Are you sure do you want to quit a game?", AcceptWindow.AcceptType.YesNo, true, (Control)MenuButtons[0]);

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