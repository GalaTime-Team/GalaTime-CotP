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
			float sceneLoadProgress = (float)sceneProgress[0] * 20f; // Scene loading is 20% of total progress
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

			// List of JSON files to load
			string[] jsonPaths = {
				GalatimeGlobals.PathListItems,
				GalatimeGlobals.PathListAbilities,
				GalatimeGlobals.PathListTips,
				GalatimeGlobals.PathListDialogs,
				GalatimeGlobals.PathListCharacters,
				GalatimeGlobals.PathListAllies,
				GalatimeGlobals.PathListElements
			};

			// Calculate progress increment
			float progressIncrement = 100f / (jsonPaths.Length + 1); // +1 for the scene

			// Load each JSON file and update progress
			foreach (string jsonPath in jsonPaths)
			{
				string fullPath = ProjectSettings.GlobalizePath(jsonPath);
				GD.Print($"Loading elements from: {fullPath}");

				if (!Godot.FileAccess.FileExists(fullPath))
				{
					GD.PrintErr($"File not found: {fullPath}");
					throw new FileNotFoundException($"File not found: {fullPath}");
				}

				// Simulate loading progress for each file
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				totalProgress += progressIncrement;
				progressBar.Value = totalProgress;
				GD.Print($"Progress: {totalProgress}%");
			}

			// Initialize global data
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
