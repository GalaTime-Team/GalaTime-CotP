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
	}

	/// <summary> Calls the given action for each page. </summary>
	public void ForEachPageControl(Action<DiaryPage> action) => Pages.ToList().ForEach(action);
	/// <summary> Returns the page with the given id. Returns null if not found. </summary>
	public DiaryPage GetPage(string id) => Pages.First(x => x.Id == id);

	// TODO: Replace with regular buttons
	public void OnButtonsInput(InputEvent @event, string id)
	{
		// Check if pressed left mouse button on the button
		if (!(@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)) return;

		var page = GetPage(id);
		ForEachPageControl(x =>
		{
			// Hide all controls
			if (x.ControlNode != null) x.ControlNode.Visible = false;

			// Change color of the all buttons
			var b = x.ButtonNode;
			if (b != null)
			{
				var t = GetTween();
				t.TweenMethod(Callable.From<Color>(x => b.AddThemeColorOverride("font_color", x)),
					b.GetThemeColor("font_color"), new Color(1f, 1f, 1f), 0.5f);
			}
		});
		// Animate the selected block
		AnimatePageButton(page);

		page.ControlNode.Visible = true;
		PageTwistAudio.Play();
	}

	// TODO: Replace buttons completely
	/// <summary> Animates the selected block to the given page. </summary>
	private void AnimatePageButton(DiaryPage page)
	{
		var btn = page.ButtonNode as Label;
		var margin = 24;

		var calculatedSize = (btn.Size * 2) with { Y = btn.Size.Y * 2.22f };
		var calculatedMargin = new Vector2(margin, margin * .22f);

		Tween = GetTween();
		Tween?.TweenMethod(Callable.From<Vector2>(x =>
			SelectedBlock.Size = x), SelectedBlock.Size, calculatedSize + calculatedMargin, 0.5f);
		Tween?.TweenMethod(Callable.From<Vector2>(x =>
			SelectedBlock.GlobalPosition = x), SelectedBlock.GlobalPosition, btn.GlobalPosition - calculatedMargin / 2, 0.5f);
		Tween?.TweenMethod(Callable.From<Color>(x => btn.AddThemeColorOverride("font_color", x)),
			btn.GetThemeColor("font_color"), new Color(0f, 0f, 0f), 0.5f);
	}
}
