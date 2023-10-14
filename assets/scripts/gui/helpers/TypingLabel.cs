using Godot;
using System;

namespace Galatime.UI.Helpers;

[Tool]
/// <summary> A custom label that displays text one character at a time, like in a typing animation. </summary>
public partial class TypingLabel : RichTextLabel
{
    [Export(PropertyHint.Range, "0.01,1,0.01")]
    /// <summary> Represents the typing speed in seconds. </summary>
    public float TypingSpeed = 0.1f;

    /// <summary> A delegate that is invoked when the typing animation is finished. </summary>
    public Action OnTypingFinished;
    /// <summary> A delegate that is invoked when a character is typed. </summary>
    public Action OnType;

    public Timer TypeTimer;

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
        if (VisibleCharacters >= StripBBCode(Text).Length)
        {
            if (Engine.IsEditorHint()) { 
                VisibleCharacters = 0;
                return;
            }

            TypeTimer.Stop();

            OnTypingFinished?.Invoke();
            return;
        }

        VisibleCharacters++;
        OnType?.Invoke();
    }

    public override void _Process(double delta)
    {
        if (TypeTimer is not null) TypeTimer.WaitTime = TypingSpeed;
    }

    public override void _Ready()
    {
        InitializeTypeTimer();
    }

    private void InitializeTypeTimer()
    {
        if (TypeTimer is not null) return;

        TypeTimer = new Timer() { 
            WaitTime = TypingSpeed,
            Name = Engine.IsEditorHint() ? "DebugTypeTimer" : "TypeTimer"
        };
        TypeTimer.Timeout += AppendLetter;
        AddChild(TypeTimer);

        StartTyping();
    }

    public void StartTyping() => TypeTimer.Start();
}
