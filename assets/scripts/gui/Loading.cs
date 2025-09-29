using Godot;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Galatime;

public partial class Loading : CanvasLayer
{
	public string sceneName = "res://assets/scenes/Lobby.tscn";
	private Godot.Collections.Array sceneProgress = new Godot.Collections.Array();
	private bool sceneLoaded = false;
	private bool elementsLoaded = false;
	private float totalProgress = 0f;
	private ProgressBar progressBar;
	private AnimationPlayer animationPlayer;
	private RichTextLabel tipsLabel;

	public override void _Ready()
	{
		progressBar = GetNode<ProgressBar>("Control/ProgressBar");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		tipsLabel = GetNode<RichTextLabel>("Control/TipsLabel");
		ResourceLoader.LoadThreadedRequest(sceneName);
		animationPlayer.Play("loading");
	}

	public override void _Process(double delta)
	{
		if (!sceneLoaded)
		{
			var status = ResourceLoader.LoadThreadedGetStatus(sceneName, sceneProgress);
			float sceneLoadProgress = (float)sceneProgress[0] * 70f; // Scene loading is 70% of total progress
			totalProgress = sceneLoadProgress;
			progressBar.Value = totalProgress;
			GD.Print($"{totalProgress}% - {status}. Scene: {sceneName}");

			if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				sceneLoaded = true;
			}
		}
		else if (sceneLoaded && !elementsLoaded)
		{
			LoadElementsWithProgress();
		}

		if (sceneLoaded && elementsLoaded)
		{
			var instance = (PackedScene)ResourceLoader.LoadThreadedGet(sceneName);
			GetTree().ChangeSceneToPacked(instance);
			QueueFree();
		}
	}

	private async void LoadElementsWithProgress()
	{
		try
		{
			GD.Print("Attempting to load elements from JSON");
			string jsonPath = ProjectSettings.GlobalizePath(GalatimeGlobals.PathListElements);
			GD.Print($"Loading elements from: {jsonPath}");
			if (!Godot.FileAccess.FileExists(jsonPath))
			{
				GD.PrintErr($"File not found: {jsonPath}");
				throw new FileNotFoundException($"File not found: {jsonPath}");
			}
			// Simulate loading progress for elements (30% of total progress)
			for (int i = 1; i <= 30; i++)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				totalProgress = 70f + i;
				progressBar.Value = totalProgress;
			}
			// Assuming GalatimeGlobals.Instance is already set
			GalatimeGlobals.Instance.InitializeGlobalData();
			elementsLoaded = true;
			GD.Print("Finished loading elements from JSON");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error loading elements: {ex.Message}");
			throw; // This will crash the game if loading fails
		}
	}
}
