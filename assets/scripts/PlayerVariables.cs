using Godot;

using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using Galatime;

namespace Galatime.Global;

/// <summary> Status of learning an ability. </summary>
public enum LearnedStatus
{
	Ok,
	NoEnoughCurrency,
	NoRequiredPath
}

/// <summary> Singleton, which contains all the player variables and methods. </summary>
public partial class PlayerVariables : Node
{
	public static PlayerVariables Instance { get; private set; }

	#region Variables

	/// <summary> Max number of inventory slots. </summary>
	public static int InventorySlots = 16;
	/// <summary> Max number of ability slots. </summary>
	public static int AbilitySlots = 3;
	/// <summary> Shows last changed inventory item index. </summary>
	public static int CurrentInventoryItem = -1;
	/// <summary> Current save to load. </summary>
	public static int CurrentSave = 0;
	/// <summary> If the save is loaded. </summary>
	public bool IsLoaded { get; private set; }

	#endregion

	#region Player Variables

	/// <summary> Inventory of the player. </summary>
	/// <remarks> Use <see cref="SetItem"/>, <see cref="RemoveItem"/> or <see cref="AddItem"/> to modify the inventory. </remarks>
	public Item[] Inventory = new Item[InventorySlots];
	/// <summary> Abilities of the player. </summary>
	/// <remarks> Use <see cref="SetAbility"/>, <see cref="RemoveAbility"/> or <see cref="LearnAbility"/> to modify the abilities. </remarks>
	public AbilityData[] Abilities = new AbilityData[AbilitySlots];
	/// <summary> List of the learned abilities of the player. </summary>
	/// <remarks> Use <see cref="LearnAbility"/> to add an ability. </remarks>
	public Godot.Collections.Array<string> LearnedAbilities = new();
	public AllyData[] Allies = new AllyData[6];
	/// <summary> List of the discovered enemies of their numeric ID. </summary>
	public List<int> DiscoveredEnemies = new();

	#endregion

	#region Events

	/// <summary> Emitted when the inventory is changed. </summary>
	public Action OnItemsChanged;
	/// <summary> Emitted when the abilities are changed. </summary>
	public Action OnAbilitiesChanged;
	/// <summary> Emitted when an ability is learned. </summary>
	public Action OnAbilityLearned;
	/// <summary> Emitted when the allies are changed. </summary>
	public Action OnAlliesChanged;
	public Action OnDiscoveredEnemiesChanged;

	/// <summary> Emitted when the player is ready. </summary>
	public Action PlayerIsReady;

	#endregion

	// TODO: REMOVE THESE LATER, BECAUSE IT'S REALLY UGLY.
	public static Action<float> OnXpChanged;
	

	/// <summary> Instance of the player in the current scene. </summary>
	public Player Player;
	/// <summary> If the save should be loaded. After loading, automatically set to false. </summary>
	public bool ShouldLoadSave = true;

	public PlayerVariables() => ResetValues();

	private void ResetValues()
	{
		Array.Fill(Inventory, new());
		Array.Fill(Abilities, new());
		Array.Fill(Allies, new());
		LearnedAbilities.Clear();
	}

	public override void _Ready()
	{
		Instance = this;

		ResetValues();

		// Initializing the inventory and abilities
		OnItemsChanged?.Invoke();
		OnAbilitiesChanged?.Invoke();
	}

	// I am not sure if this is the best way to do this. But it works. So I will leave it.
	public void SetPlayerInstance(Player instance) => Player = instance;

	public void LoadVariables(Player instance)
	{
		Player = instance;
		if (ShouldLoadSave) LoadSave();

		// Invoke the events to initialize the player and global variables
		OnItemsChanged?.Invoke();
		OnAbilitiesChanged?.Invoke();
		OnAbilityLearned?.Invoke();
		OnAlliesChanged?.Invoke();
		OnDiscoveredEnemiesChanged?.Invoke();

		PlayerIsReady?.Invoke();
	}

	/// <summary> Discover an enemy by its numeric ID. </summary>
	public void DiscoverEnemy(int id)
	{
		if (!DiscoveredEnemies.Contains(id)) DiscoveredEnemies.Add(id);
		OnDiscoveredEnemiesChanged?.Invoke();
		GD.Print("Discovered enemies: " + DiscoveredEnemies.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y));
	}

	#region Save/Load

	/// <summary> Set current save to load. </summary>
	public void SetSave(int save)
	{
		CurrentSave = save;
		ShouldLoadSave = true;
		LevelManager.Instance.LevelObjects.Clear();
	}

	/// <summary> The last loaded save data. Accessible for restoring player state after scene changes. </summary>
	public SaveData LastLoadedSave { get; private set; }

	/// <summary> Loads the save from the save file using the new SaveData class. </summary>
	public void LoadSave()
	{
		ResetValues();

		try
		{
			// Get the save data using the new SaveData class
			var saveData = GalatimeGlobals.LoadSave(CurrentSave);
			LastLoadedSave = saveData;

			// Load equipped abilities
			foreach (var savedAbility in saveData.EquippedAbilities)
			{
				if (!string.IsNullOrEmpty(savedAbility.ID) && savedAbility.Slot >= 0 && savedAbility.Slot < Abilities.Length)
				{
					Abilities[savedAbility.Slot] = GalatimeGlobals.GetAbilityById(savedAbility.ID);
				}
			}

			// Load inventory items
			foreach (var savedItem in saveData.Inventory)
			{
				if (!string.IsNullOrEmpty(savedItem.ID) && savedItem.Slot >= 0 && savedItem.Slot < Inventory.Length)
				{
					var item = GalatimeGlobals.GetItemById(savedItem.ID);
					if (item != null && !item.IsEmpty)
					{
						item.Quantity = savedItem.Quantity;
						Inventory[savedItem.Slot] = item;
					}
				}
			}

			// Load allies - if none saved, add default characters (arthur & raphael)
			if (saveData.Allies.Count > 0)
			{
				for (int i = 0; i < saveData.Allies.Count && i < Allies.Length; i++)
				{
					var allyId = saveData.Allies[i];
					if (!string.IsNullOrEmpty(allyId))
					{
						Allies[i] = GalatimeGlobals.GetAllyById(allyId);
					}
				}
			}
			else
			{
				// New game - add default characters
				InitializeDefaultAllies();
			}

			// Load discovered enemies
			DiscoveredEnemies.Clear();
			foreach (var enemyId in saveData.DiscoveredEnemies)
			{
				if (!DiscoveredEnemies.Contains(enemyId))
				{
					DiscoveredEnemies.Add(enemyId);
				}
			}

			// Load player XP
			if (Player != null)
			{
				Player.Xp = saveData.PlayerState.Xp;
			}

			// Load learned abilities
			LearnedAbilities.Clear();
			foreach (var abilityId in saveData.LearnedAbilities)
			{
				if (!string.IsNullOrEmpty(abilityId))
				{
					LearnedAbilities.Add(abilityId);
				}
			}

			// Load level object states into LevelManager
			if (LevelManager.Instance != null && saveData.LevelStates.Count > 0)
			{
				foreach (var levelState in saveData.LevelStates)
				{
					if (string.IsNullOrEmpty(levelState.LevelName)) continue;
					
					var levelObjects = new System.Collections.Generic.List<LevelObject>();
					foreach (var savedObj in levelState.Objects)
					{
						levelObjects.Add(new LevelObject(savedObj.Name, savedObj.Data));
					}
					
					if (!LevelManager.Instance.LevelObjects.ContainsKey(levelState.LevelName))
					{
						LevelManager.Instance.LevelObjects.Add(levelState.LevelName, levelObjects);
					}
					else
					{
						LevelManager.Instance.LevelObjects[levelState.LevelName] = levelObjects;
					}
				}
			}

			// Set spawn point index
			if (LevelManager.Instance != null)
			{
				LevelManager.Instance.PlayerSpawnPointIndex = saveData.SpawnPointIndex;
			}

			ShouldLoadSave = false;
			IsLoaded = true;
			
			GD.PrintRich("[color=green]SAVE SYSTEM[/color]: Save loaded successfully");
		}
		catch (Exception e)
		{
			GD.PrintRich("[color=red]SAVE SYSTEM[/color]: Error when loading save");
			GD.PrintRich("Message: " + e.Message);
			GD.PrintRich("Source: " + e.Source);
			GD.PrintRich("Stack Trace: " + e.StackTrace);
			
			// If loading fails, ensure default allies are still initialized
			InitializeDefaultAllies();
		}
	}

	/// <summary>
	/// Initializes the default allies (Arthur and Raphael) for a new game.
	/// </summary>
	private void InitializeDefaultAllies()
	{
		// Add Arthur as the main character
		var arthur = GalatimeGlobals.GetAllyById("arthur");
		if (arthur != null && !arthur.IsEmpty)
		{
			Allies[0] = arthur;
			GD.PrintRich("[color=green]SAVE SYSTEM[/color]: Added default ally: Arthur");
		}
		else
		{
			GD.PrintErr("SAVE SYSTEM: Failed to load default ally 'arthur'. Check allies.json configuration.");
		}
		
		// Add Raphael as the second character
		var raphael = GalatimeGlobals.GetAllyById("raphael");
		if (raphael != null && !raphael.IsEmpty)
		{
			Allies[1] = raphael;
			GD.PrintRich("[color=green]SAVE SYSTEM[/color]: Added default ally: Raphael");
		}
		else
		{
			GD.PrintErr("SAVE SYSTEM: Failed to load default ally 'raphael'. Check allies.json configuration.");
		}
	}

	/// <summary>
	/// Restores the player's character state (health, mana, stamina, position) from the last loaded save.
	/// Should be called after the character is fully initialized.
	/// </summary>
	public void RestorePlayerState()
	{
		if (LastLoadedSave == null || Player == null) return;
		
		if (Player.CurrentCharacter != null)
		{
			// Restore health only if save has valid health data
			if (LastLoadedSave.PlayerState.Health > 0)
			{
				Player.CurrentCharacter.Health = LastLoadedSave.PlayerState.Health;
			}
			
			// Restore mana and stamina
			if (Player.CurrentCharacter.Mana != null)
			{
				Player.CurrentCharacter.Mana.Value = LastLoadedSave.PlayerState.Mana;
			}
			if (Player.CurrentCharacter.Stamina != null)
			{
				Player.CurrentCharacter.Stamina.Value = LastLoadedSave.PlayerState.Stamina;
			}
			
			// Restore position if save has valid position data
			if (LastLoadedSave.PlayerState.HasSavedPosition)
			{
				var savedPosition = new Godot.Vector2(
					LastLoadedSave.PlayerState.PositionX,
					LastLoadedSave.PlayerState.PositionY
				);
				Player.CurrentCharacter.GlobalPosition = savedPosition;
				GD.PrintRich($"[color=green]SAVE SYSTEM[/color]: Restored player position to ({savedPosition.X}, {savedPosition.Y})");
			}
		}
	}

	#endregion

	#region Abilities

	/// <summary> Checks if ability is learned </summary>
	/// <param name="abilityName"> ID of the ability </param>
	public bool AbilityIsLearned(string abilityName) => LearnedAbilities.FirstOrDefault(name => name == abilityName) != null;

	/// <summary> Learns an ability that can then be accessed by the player </summary>
	/// <param name="abilityName"> ID of the ability </param>
	/// <param name="test"> If true, it will only check if the ability is learnable, but not actually learn it. </param>
	/// <returns> The status of the learning. </returns>
	public LearnedStatus LearnAbility(AbilityData abilityData, bool test = false)
	{
		// Check for required abilities. 
		if (abilityData.RequiredIDs.Length >= 0)
		{
			// Check if all required abilities are learned by goes through all of them.
			foreach (var req in abilityData.RequiredIDs) if (!AbilityIsLearned(req)) return LearnedStatus.NoRequiredPath;
		}

		// Check if player has enough XP to learn the ability.
		if (Player.Xp - abilityData.CostXP < 0) return LearnedStatus.NoEnoughCurrency;

		// Learn the ability and add it to learned abilities.
		if (!test)
		{
			Player.Xp -= abilityData.CostXP;
			LearnedAbilities.Add(abilityData.ID);
			OnAbilityLearned?.Invoke();
		}

		return LearnedStatus.Ok;
	}

	/// <summary> Sets ability to new slot. </summary>
	/// <param name="ability"> JSON representation of ability data. </param>
	public void SetAbility(AbilityData ability, int slot)
	{
		if (Abilities.Length > AbilitySlots)
		{
			GD.Print("Can't set ability up to " + Abilities.Length);
			return;
		}
		Abilities[slot] = ability;
		OnAbilitiesChanged?.Invoke();
	}

	/// <summary> Removes ability item from slot. </summary>
	/// <param name="slot"> Item slot to delete. </param>
	/// <returns> Previous ability. </returns>
	public AbilityData RemoveAbility(int slot)
	{
		// Get pervious item to return
		var previousItem = new AbilityData();
		if (!Abilities[slot].IsEmpty) previousItem = Abilities[slot];
		// Remove item
		Abilities[slot] = new();
		// Send item_changed signal to GUI
		OnAbilitiesChanged?.Invoke();
		return previousItem;
	}

	#endregion

	#region Inventory

	/// <summary> Add item to free slot in the inventory. </summary>
	public void AddItem(Item item, int quantity)
	{
		if (item == null) return;

		var itm = item.Clone();

		// GD.Print($"ADD ITEM: {quantity}. {itm.Name}");

		for (var i = 0; i < Inventory.Length; i++)
		{
			var existedItem = Inventory[i];
			if
			(
				itm.Stackable &&
				!existedItem.IsEmpty && existedItem.ID == itm.ID &&
				!existedItem.StackIsFull
			)
			{
				// GD.Print($"STACKABLE ITEM FOUND IN SLOT {i} ({existedItem.Name}). Adding {quantity}");
				// Add quantity if find a similar item  
				existedItem.Quantity += quantity;
				if (quantity > item.StackSize) AddItem(itm, quantity - item.StackSize);
				return;
			}
		}

		// GD.Print($"THIS'S NOT A STACKABLE OR ITEM DOESN'T EXIST ITEM. ADDING TO EMPTY SLOT");

		// If there is no stackable item, then add it to any free slot
		for (int i = 0; i < Inventory.Length; i++)
		{
			var existedItem = Inventory[i];

			// Check if there is a free slot
			if (existedItem.IsEmpty)
			{
				// Prevent an item from being added to a weapon slot
				if (itm.Type != ItemType.WEAPON && i == 0) continue;

				// Add item
				SetItem(itm, i);

				// Check if it's stackable.
				if (itm.Stackable)
				{
					itm.Quantity = quantity;
					if (quantity > itm.StackSize) AddItem(itm, quantity - itm.StackSize);
				}

				// GD.Print($"ADDING ITEM TO SLOT {i} ({existedItem.Name}). Adding {quantity}");

				return;
			}
		}
	}

	/// <summary> Set inventory item to slot </summary>
	public Item SetItem(Item item, int slot)
	{
		// Get pervious item to return
		var previousItem = Inventory[slot];
		// Set item
		Inventory[slot] = item;
		CurrentInventoryItem = slot;
		// Send item_changed signal to GUI
		OnItemsChanged?.Invoke();
		item.OnItemChanged += () => OnItemsChanged?.Invoke();

		return previousItem;
	}

	/// <summary> Remove inventory item from slot </summary>
	public Item RemoveItem(int slot)
	{
		// Get pervious item to return
		var previousItem = Inventory[slot];
		// Remove item
		Inventory[slot] = new Item();
		// Send item_changed signal to GUI
		OnItemsChanged?.Invoke();
		return previousItem;
	}

	/// <summary> Gets all the consumables in the inventory. </summary>
	public Item[] GetConsumables()
	{
		// Copy player inventory to a new temporary list.
		var inventory = new List<Item>().Concat(GetNode<PlayerVariables>("/root/PlayerVariables").Inventory).ToList();
		// Remove all empty items and non consumable items.
		inventory.RemoveAll(item => item.IsEmpty || item.Type != ItemType.CONSUMABLE);
		return inventory.ToArray();
	}

	#endregion
}
