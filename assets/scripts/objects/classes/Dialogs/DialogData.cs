using Godot;
using System.Collections.Generic;
using Galatime.Dialogue;

using Newtonsoft.Json;

namespace Galatime.Dialogue;

/// <summary> Represents the data for all dialogs. </summary>
public class DialogsData
{
    [JsonProperty("dialogs")]
    /// <summary> The list of dialog data. </summary>
    public List<DialogData> Dialogs = new();
}

/// <summary> Represents the dialog data for a single dialog. </summary>
public class DialogData
{
    [JsonProperty("id")]
    /// <summary> The ID of the dialog. </summary>
    public string ID = "";

    [JsonProperty("lines")]
    /// <summary> The list of dialog lines in the dialog. </summary>
    public List<DialogLineData> Lines = new();
}

/// <summary> Represents a dialog line in the dialog. </summary>
public class DialogLineData
{
    /// <summary> The text of the dialog line. </summary>
    [JsonProperty("text")]
    public string Text = "";
    [JsonProperty("textspeed")]
    /// <summary> The speed of the text typing in characters per second. </summary>
    public float TextSpeed = 10f;
    [JsonProperty("autoskip")]
    /// <summary> Auto skip line in seconds. -1 means no auto skip. </summary>
    public float AutoSkip = -1;
    [JsonProperty("character")]
    /// <summary> The ID of the character speaking the dialog line. </summary>
    public string CharacterID = "";
    [JsonProperty("emotion")]
    /// <summary> The ID of the emotion associated with the dialog line. </summary>
    public string EmotionID = "";
    [JsonProperty("actions")]
    /// <summary> The list of actions associated with the dialog line. </summary>
    public List<string> Actions = new();
}

/// <summary> Represents a character. </summary>
public class DialogCharacter
{
    [JsonProperty("id")]
    /// <summary> The ID of the character. </summary>
    public string ID = "na";
    [JsonProperty("name")]
    /// <summary> The name of the character displayed on the dialog box. </summary>
    public string Name = "N/A";

    [JsonProperty("emotions")]
    /// <summary> The list of emotions sprites paths associated with the character. Key is the emotion ID and value is the path. </summary>
    public List<EmotionData> EmotionPaths = new();

    [JsonProperty("voice")]
    /// <summary> The sound voice effect associated with the character. </summary>
    public string VoicePath = "";
}

/// <summary> Represents the list of characters taken from the talking_characters.json. </summary>
public class CharactersData
{
    [JsonProperty("characters")]
    /// <summary> Gets or sets the list of characters. </summary>
    public List<DialogCharacter> Characters = new();
}

public class EmotionData
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }
}