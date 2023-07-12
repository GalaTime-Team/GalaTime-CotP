using Galatime;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public int slots = 16;
        public int abilitySlots = 3;
        public int currentItem = -1;
        public static int currentSave = 1;

        private bool _isLoaded = false;
        public bool isLoaded
        {
            get { return _isLoaded; } private set { _isLoaded = value; }
        }

        public Godot.Collections.Dictionary inventory = new Godot.Collections.Dictionary();
        public Godot.Collections.Dictionary abilities = new Godot.Collections.Dictionary();
        public Godot.Collections.Array<string> learnedAbilities = new Godot.Collections.Array<string>();
        public Timer playtimeTimer;

        [Signal] public delegate void items_changedEventHandler();
        [Signal] public delegate void abilities_changedEventHandler();
        [Signal] public delegate void ability_learnedEventHandler();

        public delegate void onXpChangedEventHandler(float amount);
        public static event onXpChangedEventHandler onXpChanged;

        [Signal] public delegate void onPlayerIsReadyEventHandler(Player instance);

        private Player _player;
        public Player Player { get; set; }

        public override void _Ready()
        {
            for (int i = 0; i < slots; i++)
            {
                inventory.Add(i, new Godot.Collections.Dictionary());
            }
            EmitSignal("items_changed", inventory);

            for (int i = 0; i < abilitySlots; i++)
            {
                abilities.Add(i, new Godot.Collections.Dictionary());
            }
            EmitSignal("abilities_changed", abilities);

            onPlayerIsReady += loadSave;
        }

        public static void invokeXpChangedEvent(float xp)
        {
            onXpChanged(xp);
        }

        public void loadSave(Player instance)
        {
            // if (isLoaded == true) return;

            try
            {
                abilities.Clear();
                for (int i = 0; i < abilitySlots; i++)
                {
                    abilities.Add(i, new Godot.Collections.Dictionary());
                }
                inventory.Clear();
                for (int i = 0; i < slots; i++)
                {
                    inventory.Add(i, new Godot.Collections.Dictionary());
                }
                learnedAbilities.Clear();

                var saveData = GalatimeGlobals.loadSave(currentSave);

                //var inventoryUndeserialized = (Godot.Collections.Dictionary)saveData["inventory"];
                //inventory = convertKeysToInt(inventoryUndeserialized);
                if (saveData.ContainsKey("equiped_abilities"))
                {
                    Godot.Collections.Dictionary abilitiesUndeserialized = (Godot.Collections.Dictionary)saveData["equiped_abilities"];
                    var abilitiesUnconverted = convertKeysToInt(abilitiesUndeserialized);
                    for (int i = 0; i < abilitiesUnconverted.Count; i++)
                    {
                        var ability = (Godot.Collections.Dictionary)abilitiesUnconverted[i];
                        if (ability.ContainsKey("id")) abilities[i] = GalatimeGlobals.getAbilityById((string)ability["id"]);
                    }
                }

                if (saveData.ContainsKey("inventory"))
                {
                    Godot.Collections.Dictionary inventoryUndeserialized = (Godot.Collections.Dictionary)saveData["inventory"];
                    var inventoryUnconverted = convertKeysToInt(inventoryUndeserialized);
                    for (int i = 0; i < inventoryUnconverted.Count; i++)
                    {
                        var item = (Godot.Collections.Dictionary)inventoryUnconverted[i];
                        if (item.ContainsKey("id"))
                        {
                            inventory[i] = GalatimeGlobals.getItemById((string)item["id"]);
                            if (item.ContainsKey("quantity")) ((Godot.Collections.Dictionary)inventory[i])["quantity"] = (Single)item["quantity"];
                        }
                    }
                }

                if (saveData.ContainsKey("stats"))
                {
                    Godot.Collections.Dictionary stats = (Godot.Collections.Dictionary)saveData["stats"];
                    foreach (string key in stats.Keys.Select(v => (string)v))
                    {
                        Player.Stats[key].Value = (int)stats[key];
                    }
                }

                Player.Xp = (int)saveData["xp"];
                learnedAbilities = (Godot.Collections.Array<string>)saveData["learned_abilities"];

                // playtimeTimer.Start();

                // GD.Print($"LOADED: {abilities}");
                GD.Print($"LOADED");

                GD.Print("GUI IS " + GetInstanceId());

                EmitSignal("items_changed");
                EmitSignal("abilities_changed");
                EmitSignal("ability_learned");

                // isLoaded = true;
            }
            catch (Exception e)
            {
                GD.PrintRich("Error when loading save: " + e.Message + e.Source + e.StackTrace);
            }
        }

        public void setPlayerInstance(Player instance)
        {
            Player = instance;
            EmitSignal("onPlayerIsReady", instance);
        }

        public Godot.Collections.Dictionary convertKeysToInt(Godot.Collections.Dictionary dict)
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
            if (id == EntityStatType.health)
            {
                Player.heal(5);
            }

            Player.Xp -= 100;
            return StatContainer.Status.ok;
        }
        /// <summary>
        /// Checks if ability is learned
        /// </summary>
        /// <param name="abilityName">ID of the ability</param>
        /// <returns></returns>
        public bool abilityIsLearned(string abilityName)
        {
            foreach (var name in learnedAbilities)
            {
                if (name == abilityName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Learns an ability that can then be accessed by the player
        /// </summary>
        /// <param name="abilityName">ID of the ability</param>
        /// <returns></returns>
        public LearnedStatus learnAbility(Godot.Collections.Dictionary abilityData, bool test = false)
        {
            if (abilityData.ContainsKey("required"))
            {
                var required = (Godot.Collections.Array<string>)abilityData["required"];
                foreach (var req in required)
                {
                    if (!abilityIsLearned(req))
                    {
                        return LearnedStatus.noRequiredPath;
                    }
                }
            }

            if (abilityData.ContainsKey("cost"))
            {
                var cost = (Single)abilityData["cost"];
                if (Player.Xp - cost <= 0)
                {
                    return LearnedStatus.noEnoughCurrency;
                }
            }

            if (!test)
            {
                Player.Xp -= (int)abilityData["cost"];
                learnedAbilities.Add((string)abilityData["id"]);
                EmitSignal(SignalName.ability_learned);
            }

            return LearnedStatus.ok;
        }

        public bool isAbilityReloaded(int id)
        {
            if (Player == null)
            {
                GD.PrintErr("Сouldn't find a player, return false"); return false;
            }

            if (Player._abiltiesReloadTimes[id] <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public int getFreeSlot()
        {
            for (var i = 0; i < inventory.Count; i++)
            {
                if (((Godot.Collections.Dictionary)inventory[i]).Count == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool isStackable(Godot.Collections.Dictionary item)
        {
            return item.ContainsKey("stackable") && (bool)item["stackable"];
        }

        public string getItemType(Godot.Collections.Dictionary item)
        {
            if (item.ContainsKey("type"))
            {
                return (string)item["type"];
            }
            else
            {
                return "default";
            }
        }

        /// <summary> Add item to free slot in inventory </summary>
        public void addItem(Godot.Collections.Dictionary item, int quantity)
        {
            // Go through all items
            for (var i = 0; i < inventory.Count; i++)
            {
                // Getting an existing Item
                var existedItem = (Godot.Collections.Dictionary)inventory[i];
                if (isStackable(item))
                {
                    // Check if there is a similar stackable item
                    if (existedItem.Count != 0 && (string)existedItem["name"] == (string)item["name"])
                    {
                        // Add quantity if find a similar item
                        setQuantity(i, quantity);
                        return;
                    }
                }
            }

            // If there is no item for stackability, then add it to any free slot

            // Go through all items
            for (int i = 0; i < inventory.Count; i++)
            {
                // Getting an existing Item
                var existedItem = (Godot.Collections.Dictionary)inventory[i];
                // Checking of existence
                if (existedItem.Count == 0)
                {
                    // Prevent an item from being added to a weapon slot
                    if (getItemType(item) == "default" && i == 0) continue;
                    setItem(item, i);
                    if (isStackable(item))
                    {
                        setQuantity(i, quantity);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Sets ability to new slot
        /// </summary>
        /// <param name="ability">JSON ability data</param>
        /// <param name="slot">Up to three slots</param>
        public void setAbility(Godot.Collections.Dictionary ability, int slot)
        {
            if (abilities.Count >= 4)
            {
                GD.Print("Can't set ability up to " + abilities.Count); return;
            }
            abilities[slot] = ability;
            EmitSignal("abilities_changed");
        }

        /// <summary>
        /// Removes ability item from slot
        /// </summary>
        /// <param name="slot">Item slot to delete</param>
        /// <returns>Previous ability</returns>
        public Godot.Collections.Dictionary removeAbility(int slot)
        {
            // Get pervious item to return
            Godot.Collections.Dictionary previousItem = new Godot.Collections.Dictionary();
            if (abilities.ContainsKey(slot)) previousItem = (Godot.Collections.Dictionary)abilities[slot];
            // Remove item
            abilities[slot] = new Godot.Collections.Dictionary();
            // Send item_changed signal to GUI
            EmitSignal("abilities_changed");
            return previousItem;
        }

        /// <summary> Set inventory item to slot </summary>
        public Godot.Collections.Dictionary setItem(Godot.Collections.Dictionary item, int slot)
        {
            // Get pervious item to return
            Godot.Collections.Dictionary previousItem = new Godot.Collections.Dictionary();
            if (inventory.ContainsKey(slot)) previousItem = (Godot.Collections.Dictionary)inventory[slot];
            // Remove item
            inventory.Remove(slot);
            // Set item
            inventory[slot] = item;
            currentItem = slot;
            // Send item_changed signal to GUI
            EmitSignal("items_changed");
            return previousItem;
        }

        /// <summary> Remove inventory item from slot </summary>
        public Godot.Collections.Dictionary removeItem(int slot)
        {
            // Get pervious item to return
            Godot.Collections.Dictionary previousItem = new Godot.Collections.Dictionary();
            if (inventory.ContainsKey(slot)) previousItem = (Godot.Collections.Dictionary)inventory[slot];
            // Remove item
            inventory[slot] = new Godot.Collections.Dictionary();
            // Send item_changed signal to GUI
            EmitSignal("items_changed");
            return previousItem;
        }

        public void setQuantity(int slot, int amount)
        {
            Godot.Collections.Dictionary item = (Godot.Collections.Dictionary)inventory[slot];
            if (item.ContainsKey("quantity")) item["quantity"] = amount + (int)item["quantity"];
            else item.Add("quantity", amount);

            GD.Print("ITEM QUANTITY: " + item["quantity"]);
            inventory[slot] = item;
            EmitSignal("items_changed");
        }
    }

}