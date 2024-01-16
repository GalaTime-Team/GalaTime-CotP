using Galatime;
using Galatime.Dialogue;
using Galatime.Global;
using Galatime.UI.Helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class DialogBox : NinePatchRect
{
    public const string DEFAULT_VOICE_PATH = "res://assets/audios/sounds/talkbeeps/lol.wav";
    public AudioStream DefaultVoice;

    private TypingLabel TypingLabel;
    private Label CharacterNameLabel;
    private TextureRect CharacterPortraitTexture;
    private AnimationPlayer SkipAnimationPlayer;

    private Player Player;

    private int currentPhrase = -1;
    /// <summary> Represents the current phrase index. </summary>
    private int CurrentPhrase
    {
        get => currentPhrase;
        set
        {
            if (CanSkip && IsDialog) currentPhrase = value; else return;

            if (value < 0) return;

            // Check if the lines are ended, if so end the dialog.
            if (CurrentDialog is not null && currentPhrase >= CurrentDialog.Lines.Count)
            {
                EndDialog();
                return;
            }

            // Move to the next phrase.
            NextPhrase(value);
        }
    }

    public static readonly Vector2 DefaultSize = new(196, 56);
    public static readonly Vector2 NASize = new(272, 72);

    /// <summary> Represents the current dialog. </summary>
    private DialogData CurrentDialog;
    /// <summary> Indicates whether skipping dialog line is allowed. </summary>
    public bool CanSkip = false;
    /// <summary> Indicates whether the dialog is ended. </summary>
    public bool IsDialog = false;

    public Action OnDialogEndCallback;

    private AudioStreamPlayer DialogAudio;
    public override void _Ready()
    {
        DefaultVoice = GD.Load<AudioStream>(DEFAULT_VOICE_PATH);

        TypingLabel = GetNode<TypingLabel>("TypingLabel");
        CharacterPortraitTexture = GetNode<TextureRect>("CharacterPortrait");
        DialogAudio = GetNode<AudioStreamPlayer>("Voice");
        SkipAnimationPlayer = GetNode<AnimationPlayer>("SkipAnimationPlayer");
        CharacterNameLabel = GetNode<Label>("CharacterName");

        TypingLabel.OnTypingFinished += StopAndPlaySkipAnimation;
        TypingLabel.TypeTimer.Stop();
    }

    /// <summary> Start a dialog by registered dialog id in the game. </summary>
    /// <remarks> If the dialog is not exist, the dialog will be ended. Dialogs taken from the <see cref="GalatimeGlobals.DialogsList"/> </remarks>
    public void StartDialog(string id, Action dialogEndCallback = null)
    {
        var dialog = GalatimeGlobals.GetDialogById(id);
        if (dialog is not null) StartDialog(dialog, dialogEndCallback);
        else
        {
            EndDialog();
        }
    }

    /// <summary> Show a simple message as a dialog. </summary>
    public void MessageDialog(string text)
    {
        var dialog = new DialogData
        {
            Lines = new List<DialogLineData>
            {
                new()
                {
                    Text = text
                }
            }
        };

        StartDialog(dialog);
    }

    public void StartDialog(DialogData dialog, Action dialogEndCallback = null)
    {
        CurrentDialog = dialog;

        TypingLabel.Text = "";
        Visible = true;
        CanSkip = true;
        IsDialog = true;

        OnDialogEndCallback = dialogEndCallback;

        CurrentPhrase++;
    }

    public void EndDialog()
    {
        Visible = false;
        CanSkip = false;
        IsDialog = false;
        ResetValues();
    }

    private void StopAndPlaySkipAnimation()
    {
        SkipAnimationPlayer.Play("loop");
        CanSkip = true;

    }

    public void StartTyping() => TypingLabel.StartTyping();

    public void NextPhrase(int phraseId)
    {
        var phrase = CurrentDialog.Lines[phraseId];

        if (phrase.Actions.Count > 0) ExecuteActions(phrase.Actions);

        // Check if the phrase is empty to not play empty lines.
        if (phrase.Text.Length == 0)
        {
            CurrentPhrase++;
            return;
        }

        CanSkip = false;
        SkipAnimationPlayer.Play("start");

        var character = GalatimeGlobals.GetCharacterById(phrase.CharacterID);

        // Set the character portrait and name, but only if the character is not N/A, because it's means that the character is not exist.
        TypingLabel.Size = character.ID == "na" ? NASize : DefaultSize;
        CharacterNameLabel.Text = character.ID == "na" ? "" : character.Name;

        TypingLabel.VisibleCharacters = 0;
        TypingLabel.Text = phrase.Text;

        var emotion = character.EmotionPaths.Find(x => x.Id == phrase.EmotionID);
        var voice = character.VoicePath != "" ? GD.Load<AudioStream>(character.VoicePath) : DefaultVoice;

        TypingLabel.SetTypingAudio(voice);

        // Determine if character is animated or not.
        Texture2D texture = GD.Load<Texture2D>(emotion.Path);
        if (texture is AnimatedTexture animatedTexture)
        {
            animatedTexture.CurrentFrame = 0;
            CharacterPortraitTexture.Texture = animatedTexture;
        }
        else CharacterPortraitTexture.Texture = texture;

        StartTyping();
    }

    public void ExecuteActions(List<string> actions)
    {
        var str = new StringBuilder();
        foreach (var action in actions) str.Append(action).Append(", ");
        GD.Print(str);

        foreach (var action in actions)
        {
            switch (action)
            {
                case "cutscene":
                    CutsceneManager.Instance.StartCutscene(actions[1]);
                    break;
            }
        }
    }

    public void SetCameraOffset(string x, string y)
    {
        Player.CameraOffset.X = int.Parse(x);
        Player.CameraOffset.Y = int.Parse(y);
    }

    public void ToggleMove() => Player.CanMove = !Player.CanMove;

    private void ResetValues()
    {
        TypingLabel.Text = "";
        TypingLabel.VisibleCharacters = -1;

        CurrentDialog = null;
        CanSkip = false;
        currentPhrase = -1;

        OnDialogEndCallback?.Invoke();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept")) CurrentPhrase++;
    }
}
