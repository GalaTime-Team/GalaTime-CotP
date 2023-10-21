using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using ExtensionMethods;

namespace Galatime
{
    public enum LearnedStatus
    {
        ok,
        noEnoughCurrency,
        noRequiredPath
    }

    public partial class PlayerVariables : Node
    {
        public static int slots = 16;
        public static int abilitySlots = 5;
        public static int currentItem = -1;
        public static int currentSave = 1;

        private bool _isLoaded = false;
        public bool isLoaded
        {
            get { return _isLoaded; }
            private set { _isLoaded = value; }
        }

        public List<Item> inventory = new();
        public List<AbilityData> abilities = new();
        public Godot.Collections.Array<string> learnedAbilities = new Godot.Collections.Array<string>();
        public Timer playtimeTimer;

        public Action OnItemsChanged;
        public Action OnAbilitiesChanged;
        public Action OnAbilityLearned;

        public delegate void onXpChangedEventHandler(float amount);
        public static event onXpChangedEventHandler onXpChanged;

        public Action<Player> OnPlayerIsReady;
        public Player Player;

        public override void _Ready()
        {
            // Initializing the inventory and abilities
            OnItemsChanged?.Invoke();

            for (int i = 0; i < slots; i++) inventory.Add(new());
            for (int i = 0; i < abilitySlots; i++) abilities.Add(new());
            OnAbilitiesChanged?.Invoke();

            OnPlayerIsReady += loadSave;
        }

        public static void invokeXpChangedEvent(float xp)
        {
            onXpChanged(xp);
        }

        public void loadSave(Player instance)
        {
            try
            {
                abilities.Clear();
                for (int i = 0; i < abilitySlots; i++)
                {
                    abilities.Add(new());
                }
                learnedAbilities.Clear();

                var saveData = GalatimeGlobals.loadSave(currentSave);

                //var inventoryUndeserialized = (Godot.Collections.Dictionary)saveData["inventory"];
                //inventory = convertKeysToInt(inventoryUndeserialized);
                if (saveData.ContainsKey("equiped_abilities"))
                {
                    Godot.Collections.Dictionary abilitiesUndeserialized = (Godot.Collections.Dictionary)saveData["equiped_abilities"];
                    // Converting keys to int, to be able to use them as indexes and loops through them
                    var abilitiesUnconverted = ConvertKeysToInt(abilitiesUndeserialized);
                    // Lopping through the saved abilities
                    for (int i = 0; i < abilitiesUnconverted.Count; i++)
                    {
                        var ability = (Godot.Collections.Dictionary)abilitiesUnconverted[i];
                        // Checking if the save contains the ability by current index, then adding it
                        if (ability.ContainsKey("id")) abilities[i] = GalatimeGlobals.GetAbilityById((string)ability["id"]);
                    }
                }

                if (saveData.ContainsKey("inventory"))
                {
                    Godot.Collections.Dictionary inventoryUndeserialized = (Godot.Collections.Dictionary)saveData["inventory"];
                    // Converting keys to int, to be able to use them as indexes and loops through them
                    var inventoryUnconverted = ConvertKeysToInt(inventoryUndeserialized);
                    for (int i = 0; i < inventoryUnconverted.Count; i++)
                    {
                        // Checking if the save contains the item by current index
                        if (!inventoryUnconverted.ContainsKey(i)) { 
                            // If not, we add empty item (space between items)
                            inventory[i] = new Item();
                            continue;
                        }
                        // Getting the item
                        var item = (Godot.Collections.Dictionary)inventoryUnconverted[i];
                        if (item.ContainsKey("id"))
                        {
                            // Adding the item to the inventory
                            inventory[i] = GalatimeGlobals.getItemById((string)item["id"]);
                            // Adding the quantity as well
                            inventory[i].Quantity = (int)item["quantity"];
                        }
                    }
                }

                // if (saveData.ContainsKey("stats"))
                // {
                //     Godot.Collections.Dictionary stats = (Godot.Collections.Dictionary)saveData["stats"];
                //     foreach (string key in stats.Keys.Select(v => (string)v))
                //     {
                //         Player.Stats[key].Value = (int)stats[key];
                //     }
                // }

                Player.Xp = (int)saveData.GetOrNull("xp");
                learnedAbilities = (Godot.Collections.Array<string>)saveData["learned_abilities"];

                // playtimeTimer.Start();

                // Invoke the events to intalize the player and global variables
                OnItemsChanged?.Invoke();
                OnAbilitiesChanged?.Invoke();
                OnAbilitiesChanged?.Invoke();
            }
            catch (Exception e)
            {
                GD.PrintRich("Error when loading save: ");
                GD.PrintRich("Message: " + e.Message);
                GD.PrintRich("Source: " + e.Source);
                GD.PrintRich("Stack Trace: " + e.StackTrace);

                if (e.InnerException != null)
                {
                    GD.PrintRich("Inner Exception Message: " + e.InnerException.Message);
                    GD.PrintRich("Inner Exception Source: " + e.InnerException.Source);
                    GD.PrintRich("Inner Exception Stack Trace: " + e.InnerException.StackTrace);
                }
            }
        }

        // I am not sure if this is the best way to do this. But it works. So I will leave it.
        public void setPlayerInstance(Player instance)
        {
            Player = instance;
            OnPlayerIsReady?.Invoke(Player);
        }

        /// <summary>
        /// Converts dictionary keys to int. Used to be able to use keys of the dictionary as indexes.
        /// </summary>
        /// <param name="dict">The dictionary to convert</param>
        /// <returns>The converted dictionary</returns>
        public Godot.Collections.Dictionary ConvertKeysToInt(Godot.Collections.Dictionary dict)
        {
            Godot.Collections.Dictionary newDict = new Godot.Collections.Dictionary();
            foreach (var key in dict.Keys)
            {
                int newKey;
                if (int.TryParse(key.ToString(), out newKey))
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

        /// <summary>
        /// Upgrades a certain stat of an ally or player
        /// The cost is determined by the amount of upgrade, will return <c>false</c> if not enough currency or reached the maximum for the upgrade.
        /// </summary>
        /// <param name="id">Needed stats to upgrade</param>
        /// <returns>Is the upgrading successful?</returns>
        public StatContainer.Status upgradeStat(EntityStatType id)
        {
            if (Player.Xp < 100) return StatContainer.Status.noEnough;
            if (Player.Stats[id].Value >= 150) return StatContainer.Status.maximum;

            Player.Stats[id].Value += 5;
            if (id == EntityStatType.Health)
            {
                Player.Heal(5);
            }

            Player.Xp -= 100;
            return StatContainer.Status.ok;
        }
        /// <summary>
        /// Checks if ability is learned
        /// </summary>
        /// <param name="abilityName">ID of the ability</param>
        /// <returns></returns>
        public bool AbilityIsLearned(string abilityName) => learnedAbilities.FirstOrDefault(name => name == abilityName) != null;

        /// <summary>
        /// Learns an ability that can then be accessed by the player
        /// </summary>
        /// <param name="abilityName">ID of the ability</param>
        /// <returns></returns>
        public LearnedStatus LearnAbility(AbilityData abilityData, bool test = false)
        {
            if (abilityData.RequiredIDs.Length >= 0)
            {
                var required = abilityData.RequiredIDs;
                foreach (var req in required)
                {
                    if (!AbilityIsLearned(req))
                    {
                        return LearnedStatus.noRequiredPath;
                    }
                }
            }

            var cost = abilityData.CostXP;
            if (Player.Xp - cost <= 0)
            {
                return LearnedStatus.noEnoughCurrency;
            }

            if (!test)
            {
                Player.Xp -= abilityData.CostXP;
                learnedAbilities.Add(abilityData.ID);
                OnAbilityLearned?.Invoke();
            }

            return LearnedStatus.ok;
        }

        // public bool isAbilityReloaded(int id)
        // {
        //     if (Player == null)
        //     {
        //         GD.PrintErr("Сouldn't find a player, return false"); return false;
        //     }

        //     if (Player._abiltiesReloadTimes[id] <= 0)
        //     {
        //         return true;
        //     }
        //     else
        //     {
        //         return false;
        //     }
        // }

        /// <summary> Add item to free slot in inventory </summary>
        public void addItem(Item item, int quantity)
        {
            // Go through all items
            for (var i = 0; i < inventory.Count; i++)
            {
                // Getting an existing Item
                var existedItem = inventory[i];
                if (item.Stackable && existedItem.Stackable && !existedItem.IsEmpty)
                {
                    // Check if there is a similar stackable item
                    if (existedItem.ID == item.ID)
                    {
                        // Add quantity if find a similar item
                        AddQuantity(i, quantity);
                        return;
                    }
                }
            }

            // If there is no item for stackability, then add it to any free slot

            // Go through all items
            for (int i = 0; i < inventory.Count; i++)
            {
                // Getting an existing Item
                var existedItem = inventory[i];

                // Check if there is a free slot
                if (existedItem.IsEmpty)
                {
                    // Prevent an item from being added to a weapon slot
                    if (item.Type != SlotType.WEAPON && i == 0) continue;

                    // Check if it's stackable.
                    if (item.Stackable)
                    {
                        item.Quantity += quantity;
                    }
                    // Add item
                    setItem(item.Clone(), i);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets ability to new slot
        /// </summary>
        /// <param name="ability">JSON ability data</param>
        /// <param name="slot">Up to three slots</param>
        public void setAbility(AbilityData ability, int slot)
        {
            if (abilities.Count > abilitySlots)
            {
                GD.Print("Can't set ability up to " + abilities.Count);
            }
            abilities[slot] = ability;
            OnAbilitiesChanged?.Invoke();
        }

        /// <summary>
        /// Removes ability item from slot
        /// </summary>
        /// <param name="slot">Item slot to delete</param>
        /// <returns>Previous ability</returns>
        public AbilityData removeAbility(int slot)
        {
            // Get pervious item to return
            var previousItem = new AbilityData();
            if (!abilities[slot].IsEmpty) previousItem = abilities[slot];
            // Remove item
            abilities[slot] = new();
            // Send item_changed signal to GUI
            OnAbilitiesChanged?.Invoke();
            return previousItem;
        }

        /// <summary> Set inventory item to slot </summary>
        public Item setItem(Item item, int slot)
        {
            // Get pervious item to return
            var previousItem = inventory[slot];
            // Set item
            inventory[slot] = item;
            currentItem = slot;
            // Send item_changed signal to GUI
            OnItemsChanged?.Invoke();
            return previousItem;
        }

        /// <summary> Remove inventory item from slot </summary>
        public Item removeItem(int slot)
        {
            // Get pervious item to return
            var previousItem = inventory[slot];
            // Remove item
            inventory[slot] = new Item();
            // Send item_changed signal to GUI
            OnItemsChanged?.Invoke();
            return previousItem;
        }

        public void AddQuantity(int slot, int amount)
        {
            var item = inventory[slot];
            item.Quantity += amount;

            GD.Print("ITEM QUANTITY: " + item.Quantity);
            OnItemsChanged?.Invoke();
        }
    }

}