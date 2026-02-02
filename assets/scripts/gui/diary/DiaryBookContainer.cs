using System;
using System.Linq;
using Godot;

namespace Galatime.UI;

/// <summary> Represents a container for the diary book. </summary>
public partial class DiaryBookContainer : Control
{
	/// <summary> The pages that the book contains. </summary>
	[Export] public Godot.Collections.Array<DiaryPage> Pages = new();

	/// <summary> The white block that shows the selected page. </summary>
	public ColorRect SelectedBlock;
	public AudioStreamPlayer PageTwistAudio;
	public Tween Tween;

	public Tween GetTween() => GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);

	public override void _Ready()
	{
		// Get nodes
		SelectedBlock = GetNode<ColorRect>("SelectedBlock");
		PageTwistAudio = GetNode<AudioStreamPlayer>("PageTwistAudio");

		foreach (var page in Pages)
		{
			// Get nodes from the page
			page.ControlNode = GetNode<Control>(page.Control);
			page.ButtonNode = GetNode<Control>(page.Button);

			var id = page.Id; // Why C#, just why?
			// TODO: Replace with OnPressed
			page.ButtonNode.GuiInput += (InputEvent @event) => OnButtonsInput(@event, id);
		}

		// Open the first page by default (inventory) after layout is ready
		if (Pages.Count > 0)
		{
			OpenFirstPageAsync();
		}
	}
	
	/// <summary> Opens the first page after waiting for layout to be computed. </summary>
	private async void OpenFirstPageAsync()
	{
		// Wait for the next process frame to ensure layout containers have computed positions
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		
		var firstPage = Pages[0];
		var pageId = firstPage.Id;
		
		// If pageId is null or page not found, default to first page
		var page = GetPage(pageId);
		if (page == null && Pages.Count > 0)
		{
			pageId = Pages[0].Id;
			GD.Print($"Page '{pageId}' not found or null, defaulting to first page: {pageId}");
		}
		
		// On initial open, set the selection immediately without animation
		OpenPage(pageId, playSound: false, animate: false);
	}

	/// <summary> Calls the given action for each page. </summary>
	public void ForEachPageControl(Action<DiaryPage> action) => Pages.ToList().ForEach(action);
	/// <summary> Returns the page with the given id. Returns null if not found. </summary>
	public DiaryPage GetPage(string id) => Pages.FirstOrDefault(x => x.Id == id);

	/// <summary> Opens a page by its ID. </summary>
	/// <param name="id">The ID of the page to open.</param>
	/// <param name="playSound">Whether to play the page turn sound.</param>
	/// <param name="animate">Whether to animate the tab selection. Set to false for initial selection.</param>
	public void OpenPage(string id, bool playSound = true, bool animate = true)
	{
		var page = GetPage(id);
		if (page == null) 
		{
			GD.PrintErr($"Page with ID '{id}' not found.");
			return;
		}
		
		ForEachPageControl(x =>
		{
			// Hide all controls
			if (x.ControlNode != null) x.ControlNode.Visible = false;

			// Change color of the all buttons
			var b = x.ButtonNode;
			if (b != null)
			{
				if (animate)
				{
					var t = GetTween();
					t.TweenMethod(Callable.From<Color>(x => b.AddThemeColorOverride("font_color", x)),
						b.GetThemeColor("font_color"), new Color(1f, 1f, 1f), 0.5f);
				}
				else
				{
					// Set color immediately without animation
					b.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
				}
			}
		});
		// Animate or set the selected block
		AnimatePageButton(page, animate);

		page.ControlNode.Visible = true;
		if (playSound) PageTwistAudio.Play();
	}

	// TODO: Replace with regular buttons
	public void OnButtonsInput(InputEvent @event, string id)
	{
		// Check if pressed left mouse button on the button
		if (!(@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)) return;

		OpenPage(id);
	}

	// TODO: Replace buttons completely
	/// <summary> Animates or sets the selected block to the given page. </summary>
	/// <param name="page">The page to select.</param>
	/// <param name="animate">Whether to animate the selection. If false, sets position immediately.</param>
	private void AnimatePageButton(DiaryPage page, bool animate = true)
	{
		var btn = page.ButtonNode as Label;
		var margin = 24;

		// Get the scale factor from the button's parent container (PagesButtonsContainer has scale 2x)
		var parentScale = btn.GetParent<Control>().Scale;
		
		// Calculate size accounting for the parent's scale
		var scaledSize = btn.Size * parentScale;
		// Add extra height (1.11f multiplier) to provide visual padding around the text
		var calculatedSize = scaledSize with { Y = scaledSize.Y * 1.11f };
		var calculatedMargin = new Vector2(margin, margin * .22f);

		var targetSize = calculatedSize + calculatedMargin;
		
		// Get the screen position using the global transform which accounts for all transforms including scale
		var btnGlobalTransform = btn.GetGlobalTransformWithCanvas();
		var targetPosition = btnGlobalTransform.Origin - calculatedMargin / 2;

		if (animate)
		{
			Tween = GetTween();
			Tween?.TweenMethod(Callable.From<Vector2>(x =>
				SelectedBlock.Size = x), SelectedBlock.Size, targetSize, 0.5f);
			Tween?.TweenMethod(Callable.From<Vector2>(x =>
				SelectedBlock.GlobalPosition = x), SelectedBlock.GlobalPosition, targetPosition, 0.5f);
			Tween?.TweenMethod(Callable.From<Color>(x => btn.AddThemeColorOverride("font_color", x)),
				btn.GetThemeColor("font_color"), new Color(0f, 0f, 0f), 0.5f);
		}
		else
		{
			// Set position and size immediately without animation
			SelectedBlock.Size = targetSize;
			SelectedBlock.GlobalPosition = targetPosition;
			btn.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f));
		}
	}
}
