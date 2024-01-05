using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Galatime.Global;

public class FoldedWindow
{
    public string ID;

    /// <summary> The window itself that will be shown. </summary>
    public Control ControlInstance;
    /// <summary> Action to be called when the window is requested to be closed. </summary>
    public Action RequestClose;

    public void Close() 
    {
        try { RequestClose?.Invoke(); }
        catch {}
    }
}

/// <summary> Used to manage windows and their visibility. </summary>
public partial class WindowManager : Node
{
    public static WindowManager Instance { get; private set; }
    public FoldedWindow CurrentWindow { get; private set; } = null;

    /// <summary> Returns if the window with the given id is busy. </summary>
    public bool IsBusy(string id) 
    {
        if (CurrentWindow == null) return false;
        return CurrentWindow.ID != id;
    }

    public override void _Ready()
    {
        Instance = this;
    }

    public bool OpenWindow(string id, Action closeAction = null, Control windowControl = null) 
    {
        if (IsBusy(id))
        {
            GD.Print($"Window with ID {id} is already open. Closing...");
            CloseWindow(id);
        }

        var window = new FoldedWindow() { ID = id, ControlInstance = windowControl };
        // Do not override the close action if not specified.
        if (closeAction != null) window.RequestClose = closeAction;

        CurrentWindow = window;

        return true;
    }

    public void CloseWindow(string id)
    {
        if (CurrentWindow == null) return;
        CurrentWindow?.Close();
        CurrentWindow = null;
    }
}
