using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galatime.Global;

/// <summary> A window that can be opened and closed. </summary>
public class FoldedWindow
{
    public string ID;

    /// <summary> The window itself that will be shown. </summary>
    public Control ControlInstance;
    /// <summary> Action to be called when the window is requested to be closed. </summary>
    public Action RequestClose;
    /// <summary> Whether the window can be overlap other windows. </summary>
    public bool CanOverlay = true;

    /// <summary> Request to close the window. </summary>
    public void Close()
    {
        try { RequestClose?.Invoke(); }
        catch { }
    }
}

/// <summary> Used to manage windows and their visibility. </summary>
public partial class WindowManager : Node
{
    public static WindowManager Instance { get; private set; }
    /// <summary> Currently opened window. Use <see cref="ToggleWindow"/> to open window. </summary>
    public FoldedWindow CurrentWindow { get; private set; }
    /// <summary> Blacklist of window IDs that will be closed if they try to open. </summary>
    public string[] Blacklist = Array.Empty<string>();

    public List<FoldedWindow> OpenedWindows = new();

    /// <summary> Returns if the window with the given id is opened. </summary>
    public bool IsOpened(string id) => OpenedWindows.Any(w => w.ID == id);
    public FoldedWindow GetWindow(string id) => OpenedWindows.Find(w => w.ID == id);
    public override void _Ready()
    {
        // Set the singleton instance.
        Instance = this;
    }

    /// <summary> Opens a new window and closes previous one. </summary>
    /// <param name="id"> Required. Unique ID of window. </param>
    /// <param name="state"> If the window is closed or open. If true it will close and false to open. </param>
    /// <param name="closeAction"> Callback when requested to close the window. It will hide <c>windowControl</c> by default. </param>
    /// <returns> Result of opening a window. </returns>
    public bool ToggleWindow(string id, bool state, Action closeAction = null, bool canOverlay = false)
    {
        if (state)
        {
            CloseWindow(id, false);
            GD.PrintRich($"[color=orange][WM][/color] Closing window with ID {id}... List: {string.Join(", ", OpenedWindows.Select(x => x.ID))}");
            return true;
        }

        CloseOthers(canOverlay);

        // Open the window.
        var window = new FoldedWindow() { ID = id, CanOverlay = canOverlay };
        // Do not override the close action if not specified.
        if (closeAction != null) window.RequestClose = closeAction;

        // Add the window to the list of opened windows.
        OpenedWindows.Add(window);

        GD.PrintRich($"[color=orange][WM][/color] Opening window with ID {id}... List: {string.Join(", ", OpenedWindows.Select(x => x.ID))}");

        // Success
        return true;
    }

    private void CloseOthers(bool canOverlay = false)
    {
        if (!canOverlay)
        {
            foreach (var w in OpenedWindows.ToList())
            {
                if (w.CanOverlay) continue;
                CloseWindow(w);
            }
        }
    }

    public void CloseWindow(string id, bool callback = true)
    {
        CloseWindow(GetWindow(id), callback);
    }

    public void CloseWindow(FoldedWindow window, bool callback = true)
    {
        if (window == null) return;

        OpenedWindows.Remove(window);
        if (callback) window.Close();
    }
}