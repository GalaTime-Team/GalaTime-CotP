using System;
using System.Linq;
using Galatime;
using Galatime.Dialogue;
using Galatime.UI.Helpers;
using Godot;

public partial class DialogBox : NinePatchRect
{
    public const string DEFAULT_VOICE_PATH = "res://assets/audios/sounds/talkbeeps/lol.wav";
    public AudioStream DefaultVoice;

    private TypingLabel TypingLabel;
    private Label CharacterNameLabel;
    private TextureRect CharacterPortraitTexture;
    private AnimationPlayer SkipAnimationPlayer;

    private GalatimeGlobals Globals;
    private Player Player;

    private int currentPhrase = -1;
    /// <summary> Represents the current phrase index. </summary>
    private int CurrentPhrase
    {
        get => currentPhrase;
        set
        {
            if (CanSkip) currentPhrase = value; else return;

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

    /// <summary> Represents the current dialog. </summary>
    private DialogData CurrentDialog;
    /// <summary> Indicates whether skipping dialog line is allowed. </summary>
    public bool CanSkip = false;

    private AudioStreamPlayer DialogAudio;
    public override void _Ready()
    {
        DefaultVoice = GD.Load<AudioStream>(DEFAULT_VOICE_PATH);

        TypingLabel = GetNode<TypingLabel>("TypingLabel");
        CharacterPortraitTexture = GetNode<TextureRect>("CharacterPortrait");
        DialogAudio = GetNode<AudioStreamPlayer>("Voice");
        SkipAnimationPlayer = GetNode<AnimationPlayer>("SkipAnimationPlayer");
        CharacterNameLabel = GetNode<Label>("CharacterName");

        Globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");

        TypingLabel.OnType += AppendLetter;
        TypingLabel.OnTypingFinished += StopAndPlaySkipAnimation;
    }

    public void StartDialog(string id)
    {
        var dialog = GalatimeGlobals.GetDialogById(id);
        if (dialog is not null)
        {
            CurrentDialog = dialog;

            TypingLabel.Text = "";
            Visible = true;
            CanSkip = true;

            CurrentPhrase++;
        }
        else
        {
            GD.PrintErr("DIALOG: dialog " + id + " is not exist");
            EndDialog();
        }
    }

    public void EndDialog()
    {
        Visible = false;
        CanSkip = false;
        ResetValues();
    }

    // <summary> A method that appends a letter to the TextLabel. </summary>
    private void AppendLetter() {
        var rnd = new Random();
        DialogAudio.PitchScale = (float)rnd.NextDouble() / 10 + 0.99f;
        DialogAudio.Play();
    } 

    private void StopAndPlaySkipAnimation()
    {
        SkipAnimationPlayer.Play("loop");
        CanSkip = true;
    }

    public void StartTyping() => TypingLabel.StartTyping();

    public void NextPhrase(int phraseId)
    {
        SkipAnimationPlayer.Play("start");
        CanSkip = false;

        var phrase = CurrentDialog.Lines[phraseId];
        if (phrase.Text.Length == 0)
        {
            CurrentPhrase++;
            return;
        }

        var character = GalatimeGlobals.GetCharacterById(phrase.CharacterID);

        TypingLabel.VisibleCharacters = 0;
        TypingLabel.Text = phrase.Text;

        var emotion = character.EmotionPaths.FirstOrDefault(x => x.Id == phrase.EmotionID);
        var voice = character.VoicePath != "" ? GD.Load<AudioStream>(character.VoicePath) : DefaultVoice;

        DialogAudio.Stream = voice;
        
        Texture2D texture = GD.Load<Texture2D>(emotion.Path);
        CharacterNameLabel.Text = character.Name;
        if (texture is AnimatedTexture animatedTexture)
        {
            animatedTexture.CurrentFrame = 0;
            CharacterPortraitTexture.Texture = animatedTexture;
        }
        else CharacterPortraitTexture.Texture = texture;

        StartTyping();
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

        CurrentPhrase = 0;
        CanSkip = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept")) CurrentPhrase++;
    }
}
