namespace Galatime.Dialogue;

/// <summary> Represents a dialog line in the dialog. </summary>
public class DialogLine
{
    public string Text = "You looks like you found default text! Dirty hacker!";
    /// <summary> The speed of the text typing in characters per second. </summary>
    public float TextSpeed = 10f;
    /// <summary> Auto skip text at the current time. -1 means no auto skip. </summary>
    public float AutoSkip = -1;

    // TODO: Implement classes for this.
    public string Character;
    public string Emotion;
    public string Action;
}