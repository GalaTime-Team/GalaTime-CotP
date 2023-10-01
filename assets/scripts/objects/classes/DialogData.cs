using Godot;
using System.Collections.Generic;
using Galatime.Dialogue;

namespace Galatime.Dialogue;

/// <summary> Represents the dialog data for the dialog. </summary>
public class DialogData
{
    string ID = "default";
    Dictionary<string, DialogLine> Lines = new();
}