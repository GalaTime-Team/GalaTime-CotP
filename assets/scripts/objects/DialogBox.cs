using System.Linq;
using Galatime;
using Galatime.Dialogue;
using Godot;

public partial class DialogBox : NinePatchRect
{
    private RichTextLabel TextLabel;
    private Label CharacterNameLabel;
    private TextureRect CharacterPortraitTexture;
    private AnimationPlayer SkipAnimationPlayer;
    private Timer Delay;

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
        TextLabel = GetNode<RichTextLabel>("DialogText");
        CharacterPortraitTexture = GetNode<TextureRect>("CharacterPortrait");
        DialogAudio = GetNode<AudioStreamPlayer>("Voice");
        SkipAnimationPlayer = GetNode<AnimationPlayer>("SkipAnimationPlayer");
        CharacterNameLabel = GetNode<Label>("CharacterName");

        Globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");

        Delay = new Timer
        {
            WaitTime = 0.04f,
            OneShot = false
        };
        Delay.Timeout += AppendLetter;

        AddChild(Delay);
    }

    public override void _ExitTree()
    {
        Delay.Timeout -= AppendLetter;
    }

    public void StartDialog(string id)
    {
        var dialog = GalatimeGlobals.GetDialogById(id);
        if (dialog is not null)
        {
            CurrentDialog = dialog;

            TextLabel.Text = "";
            Visible = true;
            CanSkip = true;

            CurrentPhrase += 1;
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

    // <summary> It removes all BBCode tags from the input string and returns the modified string. </summary>
    public string StripBBCode(string str)
    {
        var regex = new RegEx();
        regex.Compile("\\[.+?\\]");
        return regex.Sub(str, "", true);
    }

// <summary> A method that appends a letter to the TextLabel. </summary>
public void AppendLetter()
{
    if (TextLabel.VisibleCharacters >= StripBBCode(TextLabel.Text).Length)
    {
        StopAndPlaySkipAnimation();
        return;
    }

    TextLabel.VisibleCharacters++;
    PlayDialogAudio();
}

    private void StopAndPlaySkipAnimation()
    {
        Delay.Stop();
        SkipAnimationPlayer.Play("loop");
        CanSkip = true;
    }

    private void PlayDialogAudio() => DialogAudio.Play();
    public void StartTyping() => Delay.Start();

    public void NextPhrase(int phraseId)
    {
        SkipAnimationPlayer.Play("start");
        CanSkip = false;

        var phrase = CurrentDialog.Lines[phraseId];
        if (phrase.Text.Length == 0) { 
            CurrentPhrase++;
            return;
        }

        var character = GalatimeGlobals.GetCharacterById(phrase.CharacterID);

        TextLabel.VisibleCharacters = 0;
        TextLabel.Text = phrase.Text;

        var emotion = character.EmotionPaths.FirstOrDefault(x => x.Id == phrase.EmotionID);

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
        Delay.Stop();

        TextLabel.Text = "";
        TextLabel.VisibleCharacters = -1;

        CurrentPhrase = 0;
        CanSkip = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept")) CurrentPhrase++;
    }
}
