using Galatime;
using Godot;
using System;

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

        public static Godot.Collections.Dictionary inventory = new Godot.Collections.Dictionary();
        public static Godot.Collections.Dictionary abilities = new Godot.Collections.Dictionary();
        public static Godot.Collections.Array<string> learnedAbilities = new Godot.Collections.Array<string>();

        [Signal] public delegate void items_changedEventHandler();
        [Signal] public delegate void abilities_changedEventHandler();
        [Signal] public delegate void ability_learnedEventHandler();

        public delegate void onXpChangedEventHandler(float amount);
        public static event onXpChangedEventHandler onXpChanged;

        public static Player player;

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

            GetTree().TreeChanged += _checkForPlayer;
        }

        public static void invokeXpChangedEvent(float xp)
        {
            onXpChanged(xp);
        }

        public void _checkForPlayer()
        {
            base._EnterTree();
            if (GetTree().GetNodesInGroup("player").Count > 0 && player is null)
            {
                GD.PrintRich("[color=purple]GALATIME GLOBALS[/color]: [color=lime]Player is found[/color]");
                player = GetTree().GetNodesInGroup("player")[0] as Player;
            }
            if (GetTree().GetNodesInGroup("player").Count <= 0)
            {
                GD.PrintRich("[color=purple]GALATIME GLOBALS[/color]: [color=red]Player is not found[/color]");
            }
        }

        /// <summary>
        /// Upgrades a certain stat of an ally or player
        /// The cost is determined by the amount of upgrade, will return <c>false</c> if not enough currency or reached the maximum for the upgrade.
        /// </summary>
        /// <param name="id">Needed stats to upgrade</param>
        /// <returns>Is the upgrading successful?</returns>

        public static bool upgradeStat(EntityStatType id)
        {
            if (player.Xp < 100) return false;
            switch (id)
            {
                case EntityStatType.physicalAttack:
                    player.Stats.physicalAttack.value += 5;
                    break;
                case EntityStatType.magicalAttack:
                    player.Stats.magicalAttack.value += 5;
                    break;
                case EntityStatType.physicalDefence:
                    player.Stats.physicalDefence.value += 5;
                    break;
                case EntityStatType.magicalDefence:
                    player.Stats.magicalDefence.value += 5;
                    break;
                case EntityStatType.health:
                    player.Stats.health.value += 5;
                    break;
                case EntityStatType.mana:
                    player.Stats.mana.value += 5;
                    break;
                case EntityStatType.stamina:
                    player.Stats.stamina.value += 5;
                    break;
                case EntityStatType.agility:
                    player.Stats.agility.value += 5;
                    break;
                default:
                    break;
            }
            player.Xp -= 100;
            return true;
        }
        /// <summary>
        /// Checks if ability is learned
        /// </summary>
        /// <param name="abilityName">ID of the ability</param>
        /// <returns></returns>
        public static bool abilityIsLearned(string abilityName)
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
                if (player.Xp - cost <= 0)
                {
                    return LearnedStatus.noEnoughCurrency;
                }
            }

            if (!test)
            {
                player.Xp -= (int)abilityData["cost"];
                learnedAbilities.Add((string)abilityData["id"]);
                EmitSignal(SignalName.ability_learned);
            }

            return LearnedStatus.ok;
        }

        public static bool isAbilityReloaded(int id)
        {
            if (player == null)
            {
                GD.PrintErr("Сouldn't find a player, return false"); return false;
            }

            if (player._abiltiesReloadTimes[id] <= 0)
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
                GD.PushWarning("Can't set ability up to " + abilities.Count); return;
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