using ExtensionMethods;
using Godot;
using System;

namespace Galatime.UI.Helpers;

/// <summary> A custom label that displays text one character at a time, like in a typing animation. </summary>
public partial class TypingLabel : RichTextLabel
{
    /// <summary> Represents the typing speed in seconds. </summary>
    [Export(PropertyHint.Range, "0.01,3,0.01")] public float TypingSpeed = 0.1f;
    /// <summary> Represent the puctuation delay in seconds. It will override speed on punctuation </summary>
    [Export(PropertyHint.Range, "0.01,3,0.01")] public float PunctuationDelay = 0.25f;
    [Export] public AudioStream TypingAudio;
    /// <summary> Indicating whether the play should start automatically when it added to the scene. </summary>
    [Export] public bool PlayOnStart;

    /// <summary> A delegate that is invoked when the typing animation is finished. </summary>
    public Action OnTypingFinished;
    /// <summary> A delegate that is invoked when a character is typed. </summary>
    public Action OnType;

    public Timer TypeTimer;
    public AudioStreamPlayer TypingSoundPlayer;

    // <summary> A method that appends a letter to the TextLabel. </summary>
    public void AppendLetter()
    {
        // The stripped version of the original text without BBCode to handle the actual typing functionality.
        var text = Text.StripBBCode();
        var playSound = true;

        // If there are visible characters already, check the next character and adjust typing speed accordingly.
        if (VisibleCharacters > 0)
        {
            var nextLetter = ' ';
            // If there are still characters left, get the next character.
            if (VisibleCharacters < text.Length) nextLetter = text[VisibleCharacters];

            // Set the timer wait time based on whether the next character is punctuation or not.
            TypeTimer.WaitTime = char.IsPunctuation(nextLetter) ? PunctuationDelay : TypingSpeed;
            if (char.IsWhiteSpace(nextLetter) || char.IsPunctuation(nextLetter)) playSound = false;
        }

        // If all characters are visible, trigger the typing finished event and exit the method.
        if (VisibleCharacters >= text.Length)
        {
            OnTypingFinished?.Invoke();
            return;
        }

        if (playSound) TypingSoundPlayer.Play();
        VisibleCharacters++;
        OnType?.Invoke();

        // Restart the timer to wait before appending the next character.
        TypeTimer.Start();
    }

    public override void _Ready()
    {
        TypingSoundPlayer = GetNode<AudioStreamPlayer>("TypingSoundPlayer");
        TypingSoundPlayer.Stream = TypingAudio;
        InitializeTypeTimer();
    }

    private void InitializeTypeTimer()
    {
        if (TypeTimer is not null) return;

        TypeTimer = new Timer()
        {
            WaitTime = TypingSpeed,
            Name = "TypeTimer",
            OneShot = true
        };
        TypeTimer.Timeout += AppendLetter;
        AddChild(TypeTimer);

        if (PlayOnStart) StartTyping();
    }

    public void StartTyping(string text = "")
    {
        if (text.Length > 0) Text = text;
        TypeTimer.Start();
    }
}
