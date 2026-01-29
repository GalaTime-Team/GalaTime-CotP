using Godot;

using Galatime;
using Galatime.Dialogue;
using Galatime.Global;
using Galatime.UI;
using Galatime.UI.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class DialogBox : NinePatchRect
{
	public GameLogger Logger { get; private set; } = new GameLogger("DialogBox", GameLogger.ConsoleColor.Gray);

	public const string DEFAULT_VOICE_PATH = "res://assets/audios/sounds/talkbeeps/lol.wav";
	public AudioStream DefaultVoice;

	private TypingLabel TypingLabel;
	private Label CharacterNameLabel;
	private TextureRect CharacterPortraitTexture;
	private AnimationPlayer SkipAnimationPlayer;

	public List<LabelButton> ChoiceButtons = new();

	private Player Player;
	
	// Track previous states to restore them after dialog
	private Dictionary<Entity, bool> PreviousAIStates = new();
	private Dictionary<Entity, bool> PreviousCanMoveStates = new();
	private bool PreviousPlayerFrozen = false;
	private bool PreviousPlayerCanMove = true;
	private bool IsGameFrozenForDialog = false;

	private int currentPhrase = -1;
	/// <summary> Represents the current phrase index. </summary>
	private int CurrentPhrase
	{
		get => currentPhrase;
		set
		{
			if (CanSkip && IsDialog) currentPhrase = value; else return;

			if (value < 0) return;

			// Check if the lines are ended, if so end the dialog.
			if (CurrentDialog is not null && currentPhrase >= CurrentDialog.Lines.Count)
			{
				if (CurrentDialog.Choices.Count > 0)
					StartChoice(CurrentDialog.Choices);
				else
					EndDialog();
				return;
			}

			// Move to the next phrase.
			NextPhrase(value);
		}
	}

	public static readonly Vector2 DefaultSize = new(196, 56);
	public static readonly Vector2 DefaultPosition = new(84, 24);
	public static readonly Vector2 NASize = new(264, 72);
	public static readonly Vector2 NAPosition = new(12, 12);

	/// <summary> Represents the current dialog. </summary>
	private DialogData CurrentDialog;
	/// <summary> Indicates whether skipping dialog line is allowed. </summary>
	public bool CanSkip = false;
	/// <summary> Indicates whether the dialog is ended. </summary>
	public bool IsDialog = false;

	public Action OnDialogEndCallback;
	public Action<int> OnDialogNextPhraseCallback;

	private AudioStreamPlayer DialogAudio;
	public override void _Ready()
	{
		DefaultVoice = GD.Load<AudioStream>(DEFAULT_VOICE_PATH);

		TypingLabel = GetNode<TypingLabel>("TypingLabel");
		CharacterPortraitTexture = GetNode<TextureRect>("CharacterPortrait");
		DialogAudio = GetNode<AudioStreamPlayer>("Voice");
		SkipAnimationPlayer = GetNode<AnimationPlayer>("SkipAnimationPlayer");
		CharacterNameLabel = GetNode<Label>("CharacterName");

		// Add the choice buttons. Currently only 2.
		for (int i = 0; i < 2; i++)
			ChoiceButtons.Add(GetNode<LabelButton>($"ChoiceButtonsContainer/ChoiceButton{i + 1}"));

		TypingLabel.OnTypingFinished += StopAndPlaySkipAnimation;
		TypingLabel.TypeTimer.Stop();
	}

	/// <summary> Start a dialog by registered dialog id in the game. </summary>
	/// <remarks> If the dialog is not exist, the dialog will be ended. Dialogs taken from the <see cref="GalatimeGlobals.DialogsList"/> </remarks>
	public void StartDialog(string id, Action dialogEndCallback = null, Action<int> dialogNextPhraseCallback = null)
	{
		var dialog = GalatimeGlobals.GetDialogById(id);
		if (dialog is not null) StartDialog(dialog, dialogEndCallback, dialogNextPhraseCallback);
		else
		{
			Logger.Log($"Dialog with id {id} not found.", GameLogger.LogType.Error);
			EndDialog();
		}
	}

	public void StartDialog(DialogData dialog, Action dialogEndCallback = null, Action<int> dialogNextPhraseCallback = null)
	{
		Logger.Log($"Starting dialog: {dialog.ID}", GameLogger.LogType.Success);

		CurrentDialog = dialog;

		TypingLabel.Text = "";
		Visible = true;
		CanSkip = true;
		IsDialog = true;

		OnDialogEndCallback = dialogEndCallback;
		OnDialogNextPhraseCallback = dialogNextPhraseCallback;

		// Freeze player and entities during dialog
		FreezeGameForDialog();

		CurrentPhrase++;
	}

	public void StartChoice(List<DialogChoiceData> choices)
	{
		ResetValues();

		void b(LabelButton b, int i)
		{
			b.ButtonText = choices[i].Text;

			void PressedAction()
			{
				StartDialog(choices[i].Target);
				b.Pressed -= PressedAction; // Don't forget to remove the listener
				ChoiceButtons.ForEach(bi =>
				{
					bi.Visible = false;
					if (b.Name != bi.Name) bi.Pressed -= PressedAction;
				});
			}

			b.Pressed += PressedAction;
			b.Visible = true;
		}

		for (int i = 0; i < ChoiceButtons.Count; i++)
			b(ChoiceButtons[i], i);
	}

	public void EndDialog()
	{
		Visible = false;
		CanSkip = false;
		IsDialog = false;

		ResetValues();

		// Unfreeze player and entities after dialog
		UnfreezeGameAfterDialog();

		OnDialogEndCallback?.Invoke();
	}

	private void StopAndPlaySkipAnimation()
	{
		SkipAnimationPlayer.Play("loop");
		CanSkip = true;
	}

	public void StartTyping() => TypingLabel.StartTyping();

	public void NextPhrase(int phraseId)
	{
		var phrase = CurrentDialog.Lines[phraseId];

		if (phrase.Actions.Count > 0) ExecuteActions(phrase.Actions);

		// Check if the phrase is empty to not play empty lines.
		if (phrase.Text.Length == 0)
		{
			CurrentPhrase++;
			return;
		}

		CanSkip = false;
		SkipAnimationPlayer.Play("start");

		var character = GalatimeGlobals.GetCharacterById(phrase.CharacterID);
		if (character is not null)
		{
			// Set the character portrait and name, but only if the character is not N/A, because it's means that the character is not exist.
			TypingLabel.Size = character.ID == "" ? NASize : DefaultSize;
			TypingLabel.Position = character.ID == "" ? NAPosition : DefaultPosition;

			CharacterNameLabel.Text = character.ID == "" ? "" : character.Name;

			var emotion = character.EmotionPaths.Find(x => x.Id == phrase.EmotionID);
			var voice = character.VoicePath != "" ? GD.Load<AudioStream>(character.VoicePath) : DefaultVoice;

			TypingLabel.SetTypingAudio(voice);

			#pragma warning disable CS0618 // Type or member is obsolete. AnimatedTexture is obsolete, but I want to use it anyway.

			if (emotion != null)
			{
				Texture2D texture = GD.Load<Texture2D>(emotion.Path);
				// Determine if character is animated or not.
				if (texture is AnimatedTexture animatedTexture)
				{
					animatedTexture.CurrentFrame = 0;
					CharacterPortraitTexture.Texture = animatedTexture;
				}
				else CharacterPortraitTexture.Texture = texture;
			}
		}
		else
		{
			TypingLabel.Text = phrase.Text;

			CharacterNameLabel.Text = "";
			CharacterPortraitTexture.Texture = null;

			TypingLabel.Size = NASize;
			TypingLabel.Position = NAPosition;

			TypingLabel.SetTypingAudio(DefaultVoice);
		}

		TypingLabel.VisibleCharacters = 0;
		TypingLabel.Text = phrase.Text;

		StartTyping();

		OnDialogNextPhraseCallback?.Invoke(phraseId);
	}

	[Obsolete("Use callbacks instead.")]
	public void ExecuteActions(List<string> actions)
	{
		var str = new StringBuilder();
		foreach (var action in actions) str.Append(action).Append(", ");

		foreach (var action in actions)
		{
			switch (action)
			{
				case "cutscene":
					CutsceneManager.Instance.StartCutscene(actions[1]);
					break;
			}
		}
	}

	public void SetCameraOffset(string x, string y)
	{
		Player.CameraOffset.X = int.Parse(x);
		Player.CameraOffset.Y = int.Parse(y);
	}

	private void ResetValues()
	{
		TypingLabel.Text = "";
		TypingLabel.VisibleCharacters = -1;
		CharacterNameLabel.Text = "";
		CharacterPortraitTexture.Texture = null;

		CurrentDialog = null;
		CanSkip = false;
		currentPhrase = -1;
	}

	/// <summary> Freezes the player and all entities when dialog starts. </summary>
	private void FreezeGameForDialog()
	{
		// Don't freeze again if already frozen (e.g., during dialog choices)
		if (IsGameFrozenForDialog) return;
		
		// Get the Player instance - DialogBox is child of PlayerGui which is child of CanvasLayer which is child of Player
		if (Player == null)
		{
			// Navigate up the scene tree: DialogBox -> PlayerGui -> CanvasLayer -> Player
			var parent = GetParent(); // PlayerGui
			if (parent != null)
			{
				parent = parent.GetParent(); // CanvasLayer
				if (parent != null)
				{
					parent = parent.GetParent(); // Player
					Player = parent as Player;
				}
			}
		}

		// Freeze player and store previous state
		if (Player != null)
		{
			PreviousPlayerFrozen = Player.IsPlayerFrozen;
			Player.IsPlayerFrozen = true;
			
			if (Player.CurrentCharacter != null)
			{
				PreviousPlayerCanMove = Player.CurrentCharacter.CanMove;
				Player.CurrentCharacter.CanMove = false;
			}
		}

		// Disable AI for all entities and store previous states
		PreviousAIStates.Clear();
		PreviousCanMoveStates.Clear();
		
		var entities = LevelManager.Instance?.Entities;
		if (entities != null)
		{
			// Create a copy of the list to avoid modification during iteration
			var entitiesCopy = entities.ToList();
			foreach (var entity in entitiesCopy)
			{
				if (entity != null && !entity.DeathState && GodotObject.IsInstanceValid(entity))
				{
					// Store previous states so we can restore them
					PreviousAIStates[entity] = entity.DisableAI;
					PreviousCanMoveStates[entity] = entity.CanMove;
					
					entity.DisableAI = true;
					entity.CanMove = false;
					
					// Immediately stop any ongoing movement by clearing velocity
					// This is crucial for NPCs that set Body.Velocity in their AI
					if (entity.Body != null)
					{
						entity.Body.Velocity = Vector2.Zero;
					}
				}
			}
		}
		
		IsGameFrozenForDialog = true;
	}

	/// <summary> Unfreezes the player and all entities when dialog ends. </summary>
	private void UnfreezeGameAfterDialog()
	{
		// Only unfreeze if we actually froze the game
		if (!IsGameFrozenForDialog) return;
		
		// Restore player state
		if (Player != null)
		{
			Player.IsPlayerFrozen = PreviousPlayerFrozen;
			if (Player.CurrentCharacter != null)
			{
				Player.CurrentCharacter.CanMove = PreviousPlayerCanMove;
			}
		}

		// Restore AI and CanMove for all entities using the tracked states
		// Iterate over dictionary keys to avoid collection modification issues
		var entitiesToRestore = PreviousAIStates.Keys.ToList();
		foreach (var entity in entitiesToRestore)
		{
			// Check if entity is still valid and not freed
			if (entity != null && GodotObject.IsInstanceValid(entity) && !entity.IsQueuedForDeletion())
			{
				// Restore previous AI state
				if (PreviousAIStates.ContainsKey(entity))
				{
					entity.DisableAI = PreviousAIStates[entity];
				}
				
				// Restore previous CanMove state
				if (PreviousCanMoveStates.ContainsKey(entity))
				{
					entity.CanMove = PreviousCanMoveStates[entity];
				}
			}
		}
		
		PreviousAIStates.Clear();
		PreviousCanMoveStates.Clear();
		IsGameFrozenForDialog = false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept")) CurrentPhrase++;
	}
}
