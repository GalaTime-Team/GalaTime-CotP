using Godot;

using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;

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
    public static int AbilitySlots = 5;
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

    }

    #region Save/Load

    /// <summary> Set current save to load. </summary>
    public void SetSave(int save)
    {
        CurrentSave = save;
        ShouldLoadSave = true;
        LevelManager.Instance.LevelObjects.Clear();
    }

    // TODO: Rework this, because Godot.Collections.Dictionary is slow because of marshalling and not serializable.
    /// <summary> Loads the save from the save file. </summary>
    public void LoadSave()
    {
        ResetValues();

        try // Load the save is not critical, so exception can be ignored
        {
            // Get the save data
            var saveData = GalatimeGlobals.LoadSave(CurrentSave);

            if (saveData.ContainsKey("equipped_abilities"))
            {
                Godot.Collections.Dictionary abilitiesDeserialized = (Godot.Collections.Dictionary)saveData["equipped_abilities"];
                // Converting keys to int, to be able to use them as indexes and loops through them
                var abilitiesUnconverted = ConvertKeysToInt(abilitiesDeserialized);
                // Lopping through the saved abilities
                for (int i = 0; i < abilitiesUnconverted.Count; i++)
                {
                    var ability = (Godot.Collections.Dictionary)abilitiesUnconverted[i];
                    // Checking if the save contains the ability by current index, then adding it
                    if (ability.ContainsKey("id")) Abilities[i] = GalatimeGlobals.GetAbilityById((string)ability["id"]);
                }
            }

            if (saveData.ContainsKey("inventory"))
            {
                Godot.Collections.Dictionary inventoryDeserialized = (Godot.Collections.Dictionary)saveData["inventory"];
                // Converting keys to int, to be able to use them as indexes and loops through them
                var inventoryUnconverted = ConvertKeysToInt(inventoryDeserialized);
                for (int i = 0; i < inventoryUnconverted.Count; i++)
                {
                    // Checking if the save contains the item by current index
                    if (!inventoryUnconverted.ContainsKey(i))
                    {
                        // If not, we add empty item (space between items)
                        Inventory[i] = new Item();
                        continue;
                    }
                    // Getting the item
                    var item = (Godot.Collections.Dictionary)inventoryUnconverted[i];
                    if (item.ContainsKey("id"))
                    {
                        // Adding the item to the inventory
                        Inventory[i] = GalatimeGlobals.GetItemById((string)item["id"]);
                        // Adding the quantity as well
                        Inventory[i].Quantity = (int)item["quantity"];
                    }
                }
            }

            if (saveData.ContainsKey("allies"))
            {
                Godot.Collections.Array alliesDeserialized = (Godot.Collections.Array)saveData["allies"];
                for (int i = 0; i < alliesDeserialized.Count; i++)
                {
                    var ally = (string)alliesDeserialized[i];
                    Allies[i] = GalatimeGlobals.GetAllyById(ally);
                }
            }

            Player.Xp = (int)saveData.GetOrNull("xp");
            LearnedAbilities = (Godot.Collections.Array<string>)saveData["learned_abilities"];

            ShouldLoadSave = false;
        }
        catch (Exception e)
        {
            GD.PrintRich("Error when loading save: ");
            GD.PrintRich("Message: " + e.Message);
            GD.PrintRich("Source: " + e.Source);
            GD.PrintRich("Stack Trace: " + e.StackTrace);
        }
    }

    /// <summary> Converts dictionary keys to int. Used to be able to use keys of the dictionary as indexes. </summary>
    /// <param name="dict"> The dictionary to convert </param>
    /// <returns> The converted dictionary </returns>
    public Godot.Collections.Dictionary ConvertKeysToInt(Godot.Collections.Dictionary dict)
    {
        Godot.Collections.Dictionary newDict = new Godot.Collections.Dictionary();
        foreach (var key in dict.Keys)
        {
            if (int.TryParse(key.ToString(), out int newKey))
            {
                // GD.Print($"Trying to convert: {newKey}, {dict[key]}");
                newDict.Add(newKey, dict[key]);
                // GD.Print(newDict);
            }
            else
            {
                GD.Print("Error: Cannot convert key to int: " + key);
            }
        }
        return newDict;
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