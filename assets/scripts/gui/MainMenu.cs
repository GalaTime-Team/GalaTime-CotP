using Galatime;
using Godot;
using System;
using System.Collections.Generic;

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

    private Label VersionLabel;

    private Control AcceptContainer;

    private PackedScene SaveContainerScene;

    private Label AcceptName;
    private Label AcceptYesButton;
    private Label AcceptNoButton;

    private Label ViewSavesButton;

    private Timer DelayInteract;

    public delegate void OnAccept(bool result);
    public static OnAccept onAccept;
    public GuiInputEventHandler whenPressedYes = (InputEvent @event) =>
    {
        if (@event is InputEventMouseButton inputMouse && inputMouse.ButtonIndex == MouseButton.Left && inputMouse.Pressed) onAccept(true);
    };
    public GuiInputEventHandler whenPressedNo = (InputEvent @event) =>
    {
        if (@event is InputEventMouseButton inputMouse && inputMouse.ButtonIndex == MouseButton.Left && inputMouse.Pressed) onAccept(false);
    };

    public override void _Ready()
    {
        base._Ready();

        ParseCMDLineArgs();

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
        AcceptContainer = GetNode<Control>("AcceptContainer");

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);
        tween.TweenProperty(MainMenuControl, "modulate", new Color(1f, 1f, 1f), TransitionTime);

        DelayInteract.Start();
        InitializeSavesContainers();

        VersionLabel = GetNode<Label>("VersionLabel");

        AcceptYesButton = GetNode<Label>("AcceptContainer/VBoxContainer/HBoxContainer/Yes");
        AcceptNoButton = GetNode<Label>("AcceptContainer/VBoxContainer/HBoxContainer/No");
        AcceptYesButton.PivotOffset = new Vector2(10, 6);
        AcceptNoButton.PivotOffset = new Vector2(7, 6);

        AcceptName = GetNode<Label>("AcceptContainer/VBoxContainer/Name");

        //createSaveButton.GuiInput += (InputEvent @event) => createSaveButtonInput(createSaveButton, @event);

        UpdateVisualButtons();

        MenuButtons = GetNode("MainMenuContainer/VBoxContainer").GetChildren();

        for (int i = 0; i < MenuButtons.Count; i++)
        {
            var button = MenuButtons[i] as Label;
            if (i == 0)
            {
                button.GrabFocus();
            }
            button.GuiInput += (InputEvent @event) => MainMenuButtonInput(button, @event);
        }

        GetViewport().GuiFocusChanged += guiFocusChanged;

        GalatimeGlobals.CheckSaves();
        UpdateSaves();

        AnimationPlayer.Play("idle");

        VersionLabel.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.version}\n{GalatimeConstants.versionDescription}";

        GetTree().Root.Title = "GalaTime - Main Menu";
    }

    private void InitializeSavesContainers()
    {
        SaveContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/SaveContainer.tscn");
        ViewSavesButton = GetNode<Label>("StartMenuContainer/ViewSavesFolder");
        ViewSavesButton.GuiInput += (InputEvent @event) =>
        {
            if (@event is InputEventMouseButton @eventMouse)
            {
                if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && DelayInteract.TimeLeft <= 0)
                {
                    OS.ShellOpen(ProjectSettings.GlobalizePath(GalatimeConstants.savesPath));
                    VisualButtonInput(ViewSavesButton, @event);
                }
            }
        };
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        var levelManager = GetNode<LevelManager>("/root/LevelManager");
        levelManager.EndAudioCombat();
    }

    public void ParseCMDLineArgs()
    {
        if (GalatimeGlobals.CMDArgs.ContainsKey("save"))
        {
            // Print all values of the CMDArgs dictionary
            foreach (string key in GalatimeGlobals.CMDArgs.Keys)
            {
                GD.Print(key + ": " + GalatimeGlobals.CMDArgs[key]);
            }

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

    public bool acceptIsVisible()
    {
        if (AcceptContainer.Modulate.A == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Displays an accept dialog with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for the dialog.</param>
    /// <param name="callback">The callback function to call when the dialog is accepted or dismissed.</param>
    /// <param name="invertedColors">Whether to use inverted colors for the dialog.</param>
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

        AcceptYesButton.GuiInput -= whenPressedYes;
        AcceptNoButton.GuiInput -= whenPressedNo;

        AcceptYesButton.GuiInput += whenPressedYes;
        AcceptNoButton.GuiInput += whenPressedNo;

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

        ResetButtons(false);
    }

    //public void createSaveButtonInput(Label button, InputEvent @event)
    //{
    //    visualButtonInput(button, @event);
    //    if (@event is InputEventMouseButton @eventMouse)
    //    {
    //        if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed())
    //        {
    //            appearAccept("Do you really want to create a save?\n\nNote: only the 1-st Chapter is available now", (bool result) =>
    //            {
    //                if (result)
    //                {
    //                    GD.Print("SAVE");
    //                    var saves = GalatimeGlobals.getSaves();
    //                    GalatimeGlobals.save(saves.Count + 1);
    //                    updateSaves();
    //                    disappearAccept();
    //                    OS.ShellOpen(ProjectSettings.GlobalizePath(GalatimeConstants.savesPath));
    //                }
    //                else
    //                {
    //                    disappearAccept();
    //                }
    //            }
    //            );
    //        }
    //    }
    //}

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
        Label perviousDeleteButton = null;
        Label perviousPlayButton = null;
        for (int i = 0; i < saves.Count; i++)
        {
            var instance = SaveContainerScene.Instantiate<SaveContainer>();
            GetNode("StartMenuContainer/SavesContainer").AddChild(instance);

            Label deleteButton = instance.getDeleteButtonInstance();
            Label playButton = instance.getPlayButtonInstance();

            if (perviousDeleteButton != null && perviousPlayButton != null)
            {
                playButton.FocusNeighborTop = perviousPlayButton.GetPath();
                deleteButton.FocusNeighborTop = perviousDeleteButton.GetPath();

                perviousPlayButton.FocusNeighborBottom = playButton.GetPath();
                perviousDeleteButton.FocusNeighborBottom = deleteButton.GetPath();

                perviousDeleteButton.FocusNext = playButton.GetPath();

                if (i == saves.Count - 1)
                {
                    playButton.FocusNeighborBottom = ViewSavesButton.GetPath();
                    deleteButton.FocusNeighborBottom = ViewSavesButton.GetPath();

                    ViewSavesButton.FocusNeighborTop = playButton.GetPath();
                    ViewSavesButton.FocusNeighborRight = deleteButton.GetPath();
                    ViewSavesButton.FocusNeighborLeft = playButton.GetPath();
                }
            }

            perviousPlayButton = playButton;
            perviousDeleteButton = deleteButton;

            var save = saves[i];
            deleteButton.GuiInput += (InputEvent @event) => DeleteSaveButtonInput((int)save["id"], deleteButton, @event);
            playButton.GuiInput += (InputEvent @event) => PlayButtonInput(@event, playButton, instance.id);

            VisualButtons.Add(deleteButton);
            instance.loadData(saves[i]);
        }

        UpdateVisualButtons();
        // clearVisualButtonsSaves();
    }

    public void PlayButtonInput(InputEvent @event, Label playButtonInstance, int id)
    {
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && DelayInteract.TimeLeft <= 0)
            {
                VisualButtonInput(playButtonInstance, @eventMouse);
                AnimationPlayer.Play("start");
                AudioButtonAccept.Play();

                GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Selected save {id}, waiting for end of the animation[/color]");
                PlayerVariables.currentSave = id;

                DelayInteract.Start();
            }
        }
    }

    public void OnStartAnimationEnded()
    {
        var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
        globals.LoadScene("res://assets/scenes/Lobby.tscn");
    }

    public void DeleteSaveButtonInput(int saveId, Label button, InputEvent @event)
    {
        VisualButtonInput(button, @event);
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && DelayInteract.TimeLeft <= 0)
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
        }
    }

    public void ClearVisualButtonsSaves()
    {
        for (int i = 0; i < VisualButtons.Count; i++)
        {
            var item = VisualButtons[i];
            if (item.IsInGroup("delete"))
            {
                VisualButtons.Remove(item);
            }
        }
    }

    public void UpdateVisualButtons()
    {
        VisualButtons = GetTree().GetNodesInGroup("visual");

        for (int i = 0; i < VisualButtons.Count; i++)
        {
            if (VisualButtons[i] is Node t)
            {
                var ii = t as Label;

                ii.MouseEntered += () => VisualButtonHover(ii);
                ii.MouseExited += () => VisualButtonExited(ii);
                ii.FocusEntered += () => VisualButtonHover(ii);
                ii.FocusExited += () => VisualButtonExited(ii);
            }
        }
    }

    public void MainMenuButtonInput(Label button, InputEvent @event)
    {
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && DelayInteract.TimeLeft <= 0)
            {
                var page = (string)button.GetMeta("page");

                VisualButtonInput(button, @event);
                AudioButtonAccept.Play();

                SwitchPage(page);

                DelayInteract.Start();
            }
        }
    }

    /// <summary> Switches to the specified page. </summary>
    /// <param name="page"> The name of the page to switch to. </param>
    public void SwitchPage(string page)
    {
        IsMainMenu = false;

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        if (DelayInteract.TimeLeft > 0) return;

        GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Switch to {page} menu[/color]");
        AudioMenuWhoosh.Play();
        switch (page)
        {
            case "start":
                SwipePage(SwipeDirection.UP, MainMenuControl, StartMenuControl);

                tween.TweenCallback(Callable.From(() =>
                {
                    ViewSavesButton.GrabFocus();
                })).SetDelay(TransitionTime);
                break;
            case "settings":
                var linearTween = GetTree().CreateTween();
                linearTween.SetParallel(true);

                SwipePage(SwipeDirection.LEFT, MainMenuControl, SettingsMenuControl);

                linearTween.TweenProperty(AudioMenuMuffled, "volume_db", 0, TransitionTime / 2);
                linearTween.TweenProperty(AudioMenu, "volume_db", -80, TransitionTime);

                break;
            case "credits":
                SwipePage(SwipeDirection.DOWN, MainMenuControl, CreditsMenuControl);
                break;
            default:
                break;
        }
    }
    private enum SwipeDirection { UP, RIGHT, DOWN, LEFT }

    /// <summary> Gets the opposite swipe direction. </summary>
    private SwipeDirection GetOppositeSwipeDirection(SwipeDirection direction)
    {
        return direction switch
        {
            SwipeDirection.UP => SwipeDirection.DOWN,
            SwipeDirection.RIGHT => SwipeDirection.LEFT,
            SwipeDirection.DOWN => SwipeDirection.UP,
            SwipeDirection.LEFT => SwipeDirection.RIGHT,
            _ => SwipeDirection.UP,
        };
    }

    /// <summary>
    /// Swipes the current page to the specified direction.
    /// </summary>
    /// <param name="direction">The direction to swipe the page.</param>
    /// <param name="previousControl">The control that is currently being displayed.</param>
    /// <param name="nextControl">The control that will be displayed after the swipe.</param>
    private void SwipePage(SwipeDirection direction, Control previousControl, Control nextControl)
    {
        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        var previousPosition = previousControl.Position;

        switch (direction)
        {
            case SwipeDirection.UP:
                previousPosition.Y += MainMenuControl.Size.Y;
                break;
            case SwipeDirection.RIGHT:
                previousPosition.X -= MainMenuControl.Size.X;
                break;
            case SwipeDirection.DOWN:
                previousPosition.Y -= MainMenuControl.Size.Y;
                break;
            case SwipeDirection.LEFT:
                previousPosition.X += MainMenuControl.Size.X;
                break;
            default:
                break;
        }

        tween.TweenProperty(previousControl, "position", previousPosition, TransitionTime);
        previousPosition.X += 1152;

        tween.TweenProperty(nextControl, "position", Vector2.Zero, TransitionTime);

        CurrentPageControl = nextControl;
        CurrentSwipeDirection = direction;
    }

    public void VisualButtonHover(Label button)
    {
        if (button is null) return;
        if (DelayInteract.TimeLeft <= 0)
        {
            var tween = GetTree().CreateTween();
            tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

            button.GrabFocus();

            if (!button.HasMeta("ScaleHoverOverride"))
            {
                var newScale = Vector2.Zero;
                newScale.X = 1.25f;
                newScale.Y = 1.25f;
                tween.TweenProperty(button, "scale", newScale, 0.1f);
            }
            else
            {
                var newScale = (Vector2)button.GetMeta("ScaleHoverOverride");
                tween.TweenProperty(button, "scale", newScale, 0.1f);
            }
            if (!button.HasMeta("ColorHoverOverride"))
            {
                button.Set("theme_override_colors/font_color", new Color(1, 1, 0));
            }
            else
            {
                var newColor = (Color)button.GetMeta("ColorHoverOverride");
                button.Set("theme_override_colors/font_color", newColor);
            }
            AudioButtonHover.Play();
        }
    }

    public void VisualButtonExited(Label button)
    {
        if (button is null) return;
        if (DelayInteract.TimeLeft <= 0)
        {
            var tween = GetTree().CreateTween();
            tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

            if (!button.HasMeta("ScaleHoverExitedOverride"))
            {
                tween.TweenProperty(button, "scale", Vector2.One, 0.1f);
            }
            else
            {
                var newScale = (Vector2)button.GetMeta("ScaleHoverExitedOverride");
                tween.TweenProperty(button, "scale", newScale, 0.1f);
            }

            button.Set("theme_override_colors/font_color", new Color(1, 1, 1));
        }
    }
    /// <summary>
    /// Handles input events for a visual button.
    /// </summary>
    /// <param name="button">The visual button.</param>
    /// <param name="event">The input event.</param>
    /// <remarks>
    /// This function plays an animation when the button is pressed.
    /// </remarks>
    public void VisualButtonInput(Label button, InputEvent @event)
    {
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && DelayInteract.TimeLeft <= 0)
            {
                var tween = GetTree().CreateTween();
                tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

                button.Set("theme_override_colors/font_color", new Color(0.5f, 0.5f, 0.5f));
                tween.TweenProperty(button, "theme_override_colors/font_color", new Color(1, 1, 1), 0.6f);
            }
        }
    }

    /// <summary>
    /// Resets the visual buttons to their default state.
    /// </summary>
    /// <param name="changeMouse">Whether to change the mouse cursor shape to a pointing hand.</param>
    /// <remarks>
    /// This function resets the visual buttons to their default state, including their font color, scale, and mouse cursor shape.
    /// </remarks>
    public void ResetButtons(bool changeMouse = true)
    {
        UpdateVisualButtons();
        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);
        for (int i = 0; i < VisualButtons.Count; i++)
        {
            if (VisualButtons[i] is Node t)
            {
                var ii = t as Label;
                if (VisualButtons[i] is Node)
                {
                    if (changeMouse) ii.MouseDefaultCursorShape = CursorShape.Arrow;
                    ii.Set("theme_override_colors/font_color", new Color(1f, 1f, 1f));
                    if (changeMouse) tween.TweenCallback(Callable.From(() => ii.MouseDefaultCursorShape = CursorShape.PointingHand)).SetDelay(TransitionTime);
                    if (!ii.HasMeta("ScaleHoverExitedOverride"))
                    {
                        tween.TweenProperty(ii, "scale", Vector2.One, 0.6f);
                    }
                    else
                    {
                        var newScale = (Vector2)ii.GetMeta("ScaleHoverExitedOverride");
                        tween.TweenProperty(ii, "scale", newScale, 0.6f);
                    }
                }
            }
        }
    }

    public void ToMainMenu()
    {
        GD.PrintRich($"[color=purple]MAIN MENU[/color]: Condition checking: [color=cyan]isMainMenu? - {IsMainMenu}, acceptIsVisible? - {acceptIsVisible()}[/color]");
        if (DelayInteract.TimeLeft > 0) return;
        if (acceptIsVisible()) return;
        if (IsMainMenu && !acceptIsVisible())
        {
            appearAccept("Are you sure do you want to quit a game?", (bool result) =>
                {
                    if (result) GetTree().Quit();
                    else disappearAccept();
                }, true);
            IsMainMenu = true;
            return;
        }
        if (IsMainMenu && acceptIsVisible())
        {
            disappearAccept();
            return;
        };

        DelayInteract.Start();
        ResetButtons();

        AudioMenuWhoosh.Play();

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        var linearTween = GetTree().CreateTween();
        linearTween.SetParallel(true);

        SwipePage(GetOppositeSwipeDirection(CurrentSwipeDirection), CurrentPageControl, MainMenuControl);

        linearTween.TweenProperty(AudioMenuMuffled, "volume_db", -80, TransitionTime);
        linearTween.TweenProperty(AudioMenu, "volume_db", 0, TransitionTime / 2);

        for (int i = 0; i < MenuButtons.Count; i++)
        {
            var button = MenuButtons[i] as Label;
            if (i == 0) button.GrabFocus();
        }

        // Save settings to file when back to main menu.
        GetNode<SettingsGlobals>("/root/SettingsGlobals").SaveSettings();

        IsMainMenu = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey inputKey)
        {
            if (inputKey.IsPressed() && inputKey.Keycode == Godot.Key.Escape && DelayInteract.TimeLeft <= 0) ToMainMenu();
            if (inputKey.IsPressed() && inputKey.IsAction("ui_accept"))
            {
                var mouseEvent = new InputEventMouseButton();
                mouseEvent.ButtonIndex = MouseButton.Left;
                mouseEvent.Pressed = true;

                if (CurrentFocus != null) CurrentFocus.EmitSignal("gui_input", mouseEvent);
            }
        }
    }
}
