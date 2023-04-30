using Galatime;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

public partial class MainMenu : Control
{
    public float transitionTime = 2f;
    private bool isMainMenu = true;

    private Godot.Collections.Array<Node> mainMenuButtons;
    private Godot.Collections.Array<Node> visualButtons;

    private List<Action> visualEnteredDelegates = new List<Action>();
    private List<Action> visualExitedDelegates = new List<Action>();

    private Dictionary<Label, Action> visualButtonEnteredDelegate = new Dictionary<Label, Action>();
    private Dictionary<Label, Action> visualButtonExitedDelegate = new Dictionary<Label, Action>();

    private Control _currentFocus;
    private Control _beforePopupFocus;
    private Control _currentPageControl;
    private SwipeDirection _currentSwipeDirection;

    private AudioStreamPlayer audioButtonHover;
    private AudioStreamPlayer audioButtonAccept;
    private AudioStreamPlayer audioMenu;
    private AudioStreamPlayer audioMenuMuffled;
    private AudioStreamPlayer audioMenuWhoosh;

    private AnimationPlayer chapterAnimationPlayer;

    private HSlider musicVolumeSlider;
    private HSlider soundsVolumeSlider;
    private Label discordRichPresenceButton;
    private Label discordRichPresenceStatusLabel;

    private Control startMenu;
    private Control mainMenu;
    private Control settingsMenu;
    private Control creditsMenu;

    private Label versionLabel;

    private Control acceptContainer;

    private PackedScene saveContainerScene;

    private Label acceptName;
    private Label acceptYesButton;
    private Label acceptNoButton;
    private Label viewSavesButton;

    private Timer delayInteract;

    public delegate void OnAccept(bool result);
    public static OnAccept on_accept;
    public GuiInputEventHandler whenPressedYes = (InputEvent @event) =>
    {
        if (@event is InputEventMouseButton inputMouse && inputMouse.ButtonIndex == MouseButton.Left && inputMouse.Pressed)
        {
            on_accept(true);
        }
    };
    public GuiInputEventHandler whenPressedNo = (InputEvent @event) =>
    {
        if (@event is InputEventMouseButton inputMouse && inputMouse.ButtonIndex == MouseButton.Left && inputMouse.Pressed)
        {
            on_accept(false);
        }
    };

    public override void _Ready()
    {
        base._Ready();

        audioButtonHover = GetNode<AudioStreamPlayer>("AudioStreamPlayerButtonHover");
        audioButtonAccept = GetNode<AudioStreamPlayer>("AudioStreamPlayerButtonAccept");
        audioMenu = GetNode<AudioStreamPlayer>("AudioStreamPlayerMenu");
        audioMenuMuffled = GetNode<AudioStreamPlayer>("AudioStreamPlayerMenuMuffled");
        audioMenuWhoosh = GetNode<AudioStreamPlayer>("AudioStreamPlayerWhoosh");

        //particles = GetNode<GpuParticles2D>("Particles");
        //particles2 = GetNode<GpuParticles2D>("Particles2");

        startMenu = GetNode<Control>("StartMenuContainer");
        mainMenu = GetNode<Control>("MainMenuContainer");
        settingsMenu = GetNode<Control>("SettingsMenuContainer");
        creditsMenu = GetNode<Control>("CreditsContainer");
        acceptContainer = GetNode<Control>("AcceptContainer");

        saveContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/SaveContainer.tscn");

        delayInteract = new Timer();
        delayInteract.OneShot = true;
        delayInteract.WaitTime = transitionTime;
        AddChild(delayInteract);

        // Settings Ready

        musicVolumeSlider = GetNode<HSlider>("SettingsMenuContainer/MusicVolumeSlider");
        soundsVolumeSlider = GetNode<HSlider>("SettingsMenuContainer/SoundsVolumeSlider");
        // discordRichPresenceButton = GetNode<Label>("SettingsMenuContainer/DiscordRichPresenceToggleButton");
        // discordRichPresenceStatusLabel = GetNode<Label>("SettingsMenuContainer/DiscordRichPresenceStatusLabel");

        var settingsData = GalatimeGlobals.loadSettingsConfig();

        musicVolumeSlider.ValueChanged += (double value) => changeVolume(value, new string[] { "Music", "Muffled" }, audioButtonHover);
        soundsVolumeSlider.ValueChanged += (double value) => changeVolume(value, "Sounds", audioButtonHover);

        // discordRichPresenceButton.GuiInput += discordRichPresenceButtonInput;

        musicVolumeSlider.Value = settingsData["music_volume"].ToFloat();
        soundsVolumeSlider.Value = settingsData["sounds_volume"].ToFloat();

        // Saves Ready

        //var createSaveButton = GetNode<Label>("StartMenuContainer/CreateSaveButton");
        viewSavesButton = GetNode<Label>("StartMenuContainer/ViewSavesFolder");
        viewSavesButton.GuiInput += (InputEvent @event) =>
        {
            if (@event is InputEventMouseButton @eventMouse)
            {
                if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && delayInteract.TimeLeft <= 0)
                {
                    OS.ShellOpen(ProjectSettings.GlobalizePath(GalatimeConstants.savesPath));
                    visualButtonInput(viewSavesButton, @event);
                }
            }
        };

        versionLabel = GetNode<Label>("VersionLabel");

        acceptYesButton = GetNode<Label>("AcceptContainer/VBoxContainer/HBoxContainer/Yes");
        acceptNoButton = GetNode<Label>("AcceptContainer/VBoxContainer/HBoxContainer/No");
        acceptYesButton.PivotOffset = new Vector2(10, 6);
        acceptNoButton.PivotOffset = new Vector2(7, 6);

        acceptName = GetNode<Label>("AcceptContainer/VBoxContainer/Name");

        chapterAnimationPlayer = GetNode<AnimationPlayer>("StartMenuContainer/AnimationPlayer");

        //createSaveButton.GuiInput += (InputEvent @event) => createSaveButtonInput(createSaveButton, @event);

        updateVisualButtons();

        mainMenuButtons = GetNode("MainMenuContainer/VBoxContainer").GetChildren();


        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            var button = mainMenuButtons[i] as Label;
            if (i == 0)
            {
                button.GrabFocus();
            }
            button.GuiInput += (InputEvent @event) => mainMenuButtonInput(button, @event);
        }

        GetViewport().GuiFocusChanged += guiFocusChanged;

        GalatimeGlobals.checkSaves();
        updateSaves();

        chapterAnimationPlayer.Play("idle");

        //versionLabel.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.version}\n{GalatimeConstants.versionDescription}";
        versionLabel.Text = $"\n\n\nPROPERTY OF GALATIME TEAM";

        GetTree().Root.Title = "GalaTime - Main Menu";
    }

    public void guiFocusChanged(Control control)
    {
        if (control != null)
        {
            _currentFocus = control;
        }
    }

    public bool acceptIsVisible()
    {
        if (acceptContainer.Modulate.A == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //public void discordRichPresenceButtonInput(InputEvent @event)
    //{
    //    if (@event is InputEventMouseButton @eventMouse)
    //    {
    //        if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed())
    //        {
    //            GalatimeGlobals.DiscordActivityDisabled = !GalatimeGlobals.DiscordActivityDisabled;
    //            discordRichPresenceStatusLabel.Text = GalatimeGlobals.DiscordActivityDisabled ? "Disabled" : "Enabled";
    //        }
    //    }
    //}

    public void changeVolume(double value, string[] busses, AudioStreamPlayer testSoundPlayer = null)
    {
        foreach (var bus in busses)
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(bus), (float)Mathf.LinearToDb(value));
        }
        if (testSoundPlayer != null)
        {
            testSoundPlayer.Play();
        }
    }

    public void changeVolume(double value, string bus, AudioStreamPlayer testSoundPlayer = null)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(bus), (float)Mathf.LinearToDb(value));
        if (testSoundPlayer != null)
        {
            testSoundPlayer.Play();
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
        _beforePopupFocus = _currentFocus;
        acceptNoButton.GrabFocus();

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        acceptName.Text = reason;

        tween.TweenProperty(acceptContainer, "modulate", new Color(1, 1, 1, 1), 0.3f);

        acceptContainer.MouseFilter = MouseFilterEnum.Stop;
        acceptYesButton.MouseFilter = MouseFilterEnum.Stop;
        acceptNoButton.MouseFilter = MouseFilterEnum.Stop;

        acceptYesButton.GuiInput -= whenPressedYes;
        acceptNoButton.GuiInput -= whenPressedNo;

        acceptYesButton.GuiInput += whenPressedYes;
        acceptNoButton.GuiInput += whenPressedNo;

        if (invertedColors)
        {
            acceptYesButton.SetMeta("ColorHoverOverride", new Color(1f, 0f, 0f));
            acceptNoButton.SetMeta("ColorHoverOverride", new Color(1, 1, 0));
        }
        else
        {
            acceptYesButton.SetMeta("ColorHoverOverride", new Color(1, 1, 0));
            acceptNoButton.SetMeta("ColorHoverOverride", new Color(1f, 0f, 0f));
        }

        on_accept = callback;
    }

    /// <summary>
    /// Hides the accept dialog.
    /// </summary>
    public void disappearAccept()
    {
        _beforePopupFocus.GrabFocus();

        acceptYesButton.ReleaseFocus();
        acceptNoButton.ReleaseFocus();

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        tween.TweenProperty(acceptContainer, "modulate", new Color(1, 1, 1, 0), 0.3f);

        acceptContainer.MouseFilter = MouseFilterEnum.Ignore;
        acceptYesButton.MouseFilter = MouseFilterEnum.Ignore;
        acceptNoButton.MouseFilter = MouseFilterEnum.Ignore;

        on_accept = null;

        resetButtons(false);
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

    public void updateSaves()
    {
        var saves = GalatimeGlobals.getSaves();
        GD.PrintRich("[color=purple]MAIN MENU[/color]: [color=cyan]UPDATE SAVES[/color]");
        var savesContainers = GetNode("StartMenuContainer/SavesContainer").GetChildren();

        for (int i = 0; i < savesContainers.Count; i++)
        {
            var item = savesContainers[i];
            visualButtons.Remove(item);
            item.QueueFree();
        }
        Label perviousDeleteButton = null;
        Label perviousPlayButton = null;
        for (int i = 0; i < saves.Count; i++)
        {
            var instance = saveContainerScene.Instantiate<SaveContainer>();
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
                    playButton.FocusNeighborBottom = viewSavesButton.GetPath();
                    deleteButton.FocusNeighborBottom = viewSavesButton.GetPath();

                    viewSavesButton.FocusNeighborTop = playButton.GetPath();
                    viewSavesButton.FocusNeighborRight = deleteButton.GetPath();
                    viewSavesButton.FocusNeighborLeft = playButton.GetPath();
                }
            }

            perviousPlayButton = playButton;
            perviousDeleteButton = deleteButton;

            var save = saves[i];
            deleteButton.GuiInput += (InputEvent @event) => deleteSaveButtonInput((int)save["id"], deleteButton, @event);
            playButton.GuiInput += (InputEvent @event) => playButtonInput(@event, playButton, instance.id);

            visualButtons.Add(deleteButton);
            instance.loadData(saves[i]);
        }

        updateVisualButtons();
        // clearVisualButtonsSaves();
    }

    public void playButtonInput(InputEvent @event, Label playButtonInstance, int id)
    {
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && delayInteract.TimeLeft <= 0)
            {
                visualButtonInput(playButtonInstance, @eventMouse);
                chapterAnimationPlayer.Play("start");
                audioButtonAccept.Play();

                GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Selected save {id}, waiting for end of the animation[/color]");
                PlayerVariables.currentSave = id;

                delayInteract.Start();
            }
        }
    }

    public void _onStartAnimationEnded()
    {
        GalatimeGlobals.loadScene(this, "res://assets/scenes/Lobby.tscn");
    }

    public void deleteSaveButtonInput(int saveId, Label button, InputEvent @event)
    {
        visualButtonInput(button, @event);
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && delayInteract.TimeLeft <= 0)
            {
                appearAccept("Do you really want to delete the save?", (bool result) =>
                {
                    if (result)
                    {
                        GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Deleting save {saveId}[/color]");
                        GalatimeGlobals.createBlankSave(saveId);
                        updateSaves();
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

    public void clearVisualButtonsSaves()
    {
        for (int i = 0; i < visualButtons.Count; i++)
        {
            var item = visualButtons[i];
            if (item.IsInGroup("delete"))
            {
                visualButtons.Remove(item);
            }
        }
    }

    public void updateVisualButtons()
    {
        visualButtons = GetTree().GetNodesInGroup("visual");

        for (int i = 0; i < visualButtons.Count; i++)
        {
            if (visualButtons[i] is Node t)
            {
                var ii = t as Label;

                ii.MouseEntered += () => visualButtonHover(ii);
                ii.MouseExited += () => visualButtonExited(ii);
                ii.FocusEntered += () => visualButtonHover(ii);
                ii.FocusExited += () => visualButtonExited(ii);
            }
        }
    }

    public void mainMenuButtonInput(Label button, InputEvent @event)
    {
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && delayInteract.TimeLeft <= 0)
            {
                var page = (string)button.GetMeta("page");

                visualButtonInput(button, @event);
                audioButtonAccept.Play();

                switchPage(page);

                delayInteract.Start();
            }
        }
    }

    /// <summary>
    /// Switches to the specified page.
    /// </summary>
    /// <param name="page">
    /// The name of the page to switch to.
    /// </param>
    public void switchPage(string page)
    {
        isMainMenu = false;

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        if (delayInteract.TimeLeft > 0) return;

        GD.PrintRich($"[color=purple]MAIN MENU[/color]: [color=cyan]Switch to {page} menu[/color]");
        audioMenuWhoosh.Play();
        switch (page)
        {
            case "start":
                _swipePage(SwipeDirection.UP, mainMenu, startMenu);

                tween.TweenCallback(Callable.From(() =>
                {
                    viewSavesButton.GrabFocus();
                })).SetDelay(transitionTime);
                break;
            case "settings":
                var linearTween = GetTree().CreateTween();
                linearTween.SetParallel(true);

                _swipePage(SwipeDirection.LEFT, mainMenu, settingsMenu);

                linearTween.TweenProperty(audioMenuMuffled, "volume_db", 0, transitionTime / 2);
                linearTween.TweenProperty(audioMenu, "volume_db", -80, transitionTime);

                tween.TweenCallback(Callable.From(() =>
                {
                    musicVolumeSlider.GrabFocus();
                })).SetDelay(transitionTime);
                break;
            case "credits":
                _swipePage(SwipeDirection.DOWN, mainMenu, creditsMenu);
                break;
            default:
                break;
                // mainMenuNewPosition.Y += mainMenu.Size.Y;
                // tween.TweenProperty(mainMenu, "position", mainMenuNewPosition, transitionTime);
                // mainMenuNewPosition.X += 896;

                // var startMenuNewPosition = Vector2.Zero;
                // tween.TweenProperty(startMenu, "position", startMenuNewPosition, transitionTime);

                //tween.TweenProperty(particles, "position", mainMenuNewPosition, transitionTime);
                //tween.TweenProperty(particles2, "position", mainMenuNewPosition, transitionTime);
        }
    }

    private enum SwipeDirection { UP, RIGHT, DOWN, LEFT }

    private SwipeDirection _getOpositeSwipeDirection(SwipeDirection direction)
    {
        switch (direction)
        {
            case SwipeDirection.UP: return SwipeDirection.DOWN;
            case SwipeDirection.RIGHT: return SwipeDirection.LEFT;
            case SwipeDirection.DOWN: return SwipeDirection.UP;
            case SwipeDirection.LEFT: return SwipeDirection.RIGHT;
        }
        return SwipeDirection.UP;
    }

    /// <summary>
    /// Swipes the current page to the specified direction.
    /// </summary>
    /// <param name="direction">The direction to swipe the page.</param>
    /// <param name="previousControl">The control that is currently being displayed.</param>
    /// <param name="nextControl">The control that will be displayed after the swipe.</param>
    private void _swipePage(SwipeDirection direction, Control previousControl, Control nextControl)
    {
        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        var previousPosition = previousControl.Position;

        switch (direction)
        {
            case SwipeDirection.UP:
                previousPosition.Y += mainMenu.Size.Y;
                break;
            case SwipeDirection.RIGHT:
                previousPosition.X -= mainMenu.Size.X;
                break;
            case SwipeDirection.DOWN:
                previousPosition.Y -= mainMenu.Size.Y;
                break;
            case SwipeDirection.LEFT:
                previousPosition.X += mainMenu.Size.X;
                break;
            default:
                break;
        }

        tween.TweenProperty(previousControl, "position", previousPosition, transitionTime);
        previousPosition.X += 1152;

        tween.TweenProperty(nextControl, "position", Vector2.Zero, transitionTime);

        _currentPageControl = nextControl;
        _currentSwipeDirection = direction;
    }

    public void visualButtonHover(Label button)
    {
        if (button is null) return;
        if (delayInteract.TimeLeft <= 0)
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
            audioButtonHover.Play();
        }
    }

    public void visualButtonExited(Label button)
    {
        if (button is null) return;
        if (delayInteract.TimeLeft <= 0)
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
    public void visualButtonInput(Label button, InputEvent @event)
    {
        if (@event is InputEventMouseButton @eventMouse)
        {
            if (@eventMouse.ButtonIndex == MouseButton.Left && @eventMouse.IsPressed() && delayInteract.TimeLeft <= 0)
            {
                var tween = GetTree().CreateTween();
                tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

                button.Set("theme_override_colors/font_color", new Color(0.5f, 0.5f, 0.5f));
                tween.TweenProperty(button, "theme_override_colors/font_color", new Color(1, 1, 1), 0.6f);

                //if (!button.HasMeta("ScaleHoverExitedOverride"))
                //{
                //    tween.TweenProperty(button, "scale", Vector2.One, 0.6f);
                //}
                //else
                //{
                //    var newScale = (Vector2)button.GetMeta("ScaleHoverExitedOverride");
                //    tween.TweenProperty(button, "scale", newScale, 0.6f);
                //}
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
    public void resetButtons(bool changeMouse = true)
        {
            updateVisualButtons();
            var tween = GetTree().CreateTween();
            tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);
            for (int i = 0; i < visualButtons.Count; i++)
            {
                if (visualButtons[i] is Node t)
                {
                    var ii = t as Label;
                    if (visualButtons[i] is Node)
                    {
                        if (changeMouse) ii.MouseDefaultCursorShape = CursorShape.Arrow;
                        ii.Set("theme_override_colors/font_color", new Color(1f, 1f, 1f));
                        if (changeMouse) tween.TweenCallback(Callable.From(() => ii.MouseDefaultCursorShape = CursorShape.PointingHand)).SetDelay(transitionTime);
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

        public async void toMainMenu()
        {
            GD.PrintRich($"[color=purple]MAIN MENU[/color]: Condition checking: [color=cyan]isMainMenu? - {isMainMenu}, acceptIsVisible? - {acceptIsVisible()}[/color]");

            if (delayInteract.TimeLeft > 0) return;

            if (acceptIsVisible()) return;

            if (isMainMenu && !acceptIsVisible())
            {
                appearAccept("Are you sure do you want to quit a game?", (bool result) =>
                    {
                        if (result)
                        {
                            GetTree().Quit();
                        }
                        else
                        {
                            disappearAccept();
                        }
                    }, true);
                isMainMenu = true;
                return;
            }
            if (isMainMenu && acceptIsVisible())
            {
                disappearAccept();
                return;
            };

            GalatimeGlobals.updateSettingsConfig(this, musicVolumeSlider.Value, soundsVolumeSlider.Value, false);

            delayInteract.Start();
            resetButtons();

            audioMenuWhoosh.Play();

            var tween = GetTree().CreateTween();
            tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

            var linearTween = GetTree().CreateTween();
            linearTween.SetParallel(true);

            _swipePage(_getOpositeSwipeDirection(_currentSwipeDirection), _currentPageControl, mainMenu);
            //var mainMenuNewPosition = Vector2.Zero;
            //tween.TweenProperty(mainMenu, "position", mainMenuNewPosition, transitionTime);
            ////tween.TweenProperty(particles, "position", mainMenuNewPosition, transitionTime);
            //mainMenuNewPosition.X += 896;
            ////tween.TweenProperty(particles2, "position", mainMenuNewPosition, transitionTime);

            //var startMenuNewPosition = Vector2.Zero;
            //startMenuNewPosition.Y -= startMenu.Size.Y;
            //tween.TweenProperty(startMenu, "position", startMenuNewPosition, transitionTime);

            //var creditsMenuNewPosition = Vector2.Zero;
            //creditsMenuNewPosition.Y += startMenu.Size.Y;
            //tween.TweenProperty(creditsMenu, "position", creditsMenuNewPosition, transitionTime);

            //var settingsMenuNewPosition = Vector2.Zero;
            //settingsMenuNewPosition.X -= settingsMenu.Size.X;
            //tween.TweenProperty(settingsMenu, "position", settingsMenuNewPosition, transitionTime);

            linearTween.TweenProperty(audioMenuMuffled, "volume_db", -80, transitionTime);
            linearTween.TweenProperty(audioMenu, "volume_db", 0, transitionTime / 2);

            for (int i = 0; i < mainMenuButtons.Count; i++)
            {
                var button = mainMenuButtons[i] as Label;
                if (i == 0)
                {
                    button.GrabFocus();
                }
            }

            isMainMenu = true;
        }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey inputKey)
        {
            if (inputKey.IsPressed() && inputKey.Keycode == Godot.Key.Escape && delayInteract.TimeLeft <= 0)
            {
                toMainMenu();
            }
            if (inputKey.IsPressed() && inputKey.IsAction("ui_accept"))
            {
                var mouseEvent = new InputEventMouseButton();
                mouseEvent.ButtonIndex = MouseButton.Left;
                mouseEvent.Pressed = true;

                _currentFocus.EmitSignal("gui_input", mouseEvent);
            }
        }
    }
}
