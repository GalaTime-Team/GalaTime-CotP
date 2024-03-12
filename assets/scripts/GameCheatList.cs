using System;
using System.Linq;
using System.Text;

namespace Galatime.Global;

/// <summary> A list of game cheats that registers to the cheats menu. </summary>
public static class GameCheatList
{
    /// <summary> Initializes the game cheats. </summary>
    public static void InitializeCheats(CheatsMenu cheatsMenu)
    {
        cheatsMenu.RegisterCheat(
            new Cheat(name: "[color=yellow]Game cheats[/color]", type: Cheat.CheatType.Separator),
            new Cheat(name: "Global data", type: Cheat.CheatType.Separator),
            new Cheat("list_items", "List items", "Print list of all items", "cheat_list_items", (_, _) =>
            {
                var items = GalatimeGlobals.ItemList;
                var str = new StringBuilder();

                for (var i = 0; i < items.Count; i++) str.Append($"{items[i].ID}{(i < items.Count - 1 ? ", " : "")}");

                cheatsMenu.Log(str.ToString(), CheatsMenu.LogLevel.Result);
            }),
            new Cheat("abilities_list", "Abilities list", "Print list of all abilities", "cheat_abilities_list", (_, _) =>
            {
                var abilities = GalatimeGlobals.AbilitiesList;
                var str = new StringBuilder();

                for (var i = 0; i < abilities.Count; i++) str.Append($"{abilities[i].ID}{(i < abilities.Count - 1 ? ", " : "")}");

                cheatsMenu.Log(str.ToString(), CheatsMenu.LogLevel.Result);
            }),
            new Cheat("levels_list", "Levels list", "Print list of all registered levels", "cheat_levels_list", (_, _) =>
            {
                var levels = AssetsManager.Instance.Levels;
                var str = new StringBuilder();

                for (var i = 0; i < levels.Count; i++) str.Append($"{levels.ElementAt(i).Key}{(i < levels.Count - 1 ? ", " : "")}");

                cheatsMenu.Log(str.ToString(), CheatsMenu.LogLevel.Result);
            }),
            new Cheat(name: "Levels", type: Cheat.CheatType.Separator),
            new Cheat("clear_level_objects_data", "Clear current level objects data", "Clears all level objects data in the current level", "cheat_clear_level_objects_data", (_, _) => {
                var lm = LevelManager.Instance;
                var objs = lm.GetCurrentLevelObjects();
                for (var i = 0; i < objs.Count; i++)
                {
                    var obj = objs[i];
                    obj.Data = Array.Empty<object>();
                    lm.GetCurrentLevelObjects()[i] = obj;
                }

                cheatsMenu.Log("Cleared level objects data", CheatsMenu.LogLevel.Result);
            }),
            new Cheat("load_level", "Load level", "Loads the level with the given id. Arguments: level_id [spawn_point]", "cheat_load_level", (_, input) =>
            {
                var inputArguments = cheatsMenu.ParseCheatArguments(input, 1);

                var args = inputArguments.args;
                if (!inputArguments.result) return;

                if (args.Length > 1 && int.TryParse(args[1], out var arg)) LevelManager.Instance.PlayerSpawnPointIndex = arg;
                if (args.Length > 0)
                {
                    var level = AssetsManager.Instance.Levels.FirstOrDefault(x => x.Key.ToLower() == args[0].ToLower());
                    var levelName = level.Key;
                    var levelPath = level.Value;

                    if (!string.IsNullOrEmpty(levelPath))
                    {
                        var globals = GalatimeGlobals.Instance;
                        globals.LoadScene(levelPath.Trim());
                        cheatsMenu.Log($"Loaded level {levelName}", CheatsMenu.LogLevel.Result);
                    }
                    else cheatsMenu.Log($"Level {args[0]} is not found", CheatsMenu.LogLevel.Error);
                }
                else 
                    cheatsMenu.Log($"Level {args[0]} is not found", CheatsMenu.LogLevel.Error);
            }, Cheat.CheatType.Input),
            new Cheat(name: "Gameplay cheats", type: Cheat.CheatType.Separator),
            new Cheat("god_mode", "God mode", "Toggles god mode, which makes the all characters invulnerable to all damage.", "cheat_god_mode", (active, _) =>
            {
                var pv = PlayerVariables.Instance;
                Array.ForEach(pv.Allies, c =>
                {
                    if (c.Instance != null) c.Instance.Invincible = active;
                });
            }, Cheat.CheatType.Toggle),
            new Cheat("ai_ignore_player", "Ai ignores player", "Toggles the Ai of entities to ignore the current selected player.", "cheat_ai_ignore_player", type: Cheat.CheatType.Toggle),
            new Cheat("add_ally", "Add ally", "Add an inputted ally to the player. To update allies, use 'cheat_add_allies'.", "cheat_add_ally", (_, input) =>
            {
                var inputArguments = cheatsMenu.ParseCheatArguments(input, 1);

                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var ally = GalatimeGlobals.GetAllyById(args[0]);
                if (ally == null)
                {
                    cheatsMenu.Log($"Ally {args[0]} not found", CheatsMenu.LogLevel.Warning);
                    return;
                }

                // Add the ally to any empty slot.
                for (int i = 0; i < PlayerVariables.Instance.Allies.Length; i++)
                {
                    var a = PlayerVariables.Instance.Allies[i];
                    if (a.IsEmpty) { PlayerVariables.Instance.Allies[i] = a; break; }
                }
            }, Cheat.CheatType.Input),
            new Cheat("remove_ally", "Remove ally", "Remove an inputted ally from the player. To update allies, use 'cheat_add_allies'.", "cheat_remove_ally", (_, input) =>
            {
                var inputArguments = cheatsMenu.ParseCheatArguments(input, 1);

                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var ally = PlayerVariables.Instance.Allies.FirstOrDefault(a => a.ID == args[0]);
                if (ally == null)
                {
                    cheatsMenu.Log($"Ally {args[0]} not found", CheatsMenu.LogLevel.Warning);
                    return;
                }

                // Remove the ally from the player.
                PlayerVariables.Instance.Allies[Array.IndexOf(PlayerVariables.Instance.Allies, ally)] = new AllyData();
            }, Cheat.CheatType.Input),
            new Cheat("update_allies", "Update all allies", "Updates all possible allies to the player and load them in the scene.", "cheat_update_allies", (bool active, string input) =>
            {
                var pv = PlayerVariables.Instance;
                var player = cheatsMenu.GetPlayer();
                if (player == null) return;

                for (int i = 0; i < GalatimeGlobals.AlliesList.Count; i++)
                {
                    pv.Allies[i] = GalatimeGlobals.AlliesList[i];
                }
                player.LoadCharacters("arthur");

                cheatsMenu.Log($"Updated allies", CheatsMenu.LogLevel.Result);
            }),
            new Cheat("give_xp", "Give XP", "Give an amount of XP to the player. Arguments: amount.", "", (bool active, string input) =>
            {
                var inputArguments = cheatsMenu.ParseCheatArguments(input, 1);

                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var player = cheatsMenu.GetPlayer();
                if (player == null) return;

                var xp = int.Parse(args[0]);
                PlayerVariables.Instance.Player.Xp += xp;

                cheatsMenu.Log($"Gave {xp} XP", CheatsMenu.LogLevel.Result);
            }, Cheat.CheatType.Input),
            new Cheat("give_item", "Give item", "Give an item to the player by specifying the item ID and quantity. Arguments: item_id quantity.", "cheat_give_item", (bool active, string input) =>
            {
                var inputArguments = cheatsMenu.ParseCheatArguments(input, 2);
                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var player = cheatsMenu.GetPlayer();
                if (player == null) return;

                var itemName = args[0];
                var itemQuantity = int.Parse(args[1]);
                var item = GalatimeGlobals.GetItemById(itemName);

                if (item.IsEmpty)
                {
                    cheatsMenu.Log($"Item not found: {itemName}", CheatsMenu.LogLevel.Error);
                    return;
                }

                PlayerVariables.Instance.AddItem(item, itemQuantity);

                cheatsMenu.Log($"Gave {itemQuantity}x {item.Name}", CheatsMenu.LogLevel.Result);
            }, Cheat.CheatType.Input),
            new Cheat("reload_all", "Remove all ability cooldowns", "Removes all ability cooldowns and charges.", "cheat_reload_all", (_, _) =>
            {
                var player = cheatsMenu.GetPlayer();
                if (player == null) return;

                var playerAbilities = PlayerVariables.Instance.Abilities;
                var ca = Player.CurrentCharacter;

                Array.ForEach(playerAbilities, ability =>
                {
                    ability.Charges = ability.MaxCharges;
                    ca.OnAbilityReload?.Invoke(Array.IndexOf(playerAbilities, ability), 0);
                });

                cheatsMenu.Log($"Reloaded all abilities", CheatsMenu.LogLevel.Result);
            }),
            new Cheat("restore_all", "Restore all resources", "Restores all health, mana, and stamina.", "cheat_restore_all", (_, _) =>
            {
                var player = cheatsMenu.GetPlayer();
                if (player == null) return;

                var ally = Player.CurrentCharacter;

                ally.Health = ally.Stats[EntityStatType.Health].Value;
                ally.Mana.Value = ally.Stats[EntityStatType.Mana].Value;
                ally.Stamina.Value = ally.Stats[EntityStatType.Stamina].Value;

                cheatsMenu.Log($"Restored all resources for player", CheatsMenu.LogLevel.Result);
            }),
            new Cheat(name: "Entities cheats", type: Cheat.CheatType.Separator),
            new Cheat("disable_ai", "Disable entities AI", "Disables AI for all entities, meaning that they will not perform any actions.", "cheat_disable_ai", (bool active, string input) => LevelManager.Instance.Entities.ForEach(entity => entity.DisableAI = active), Cheat.CheatType.Toggle),
            new Cheat("kill_all_enemies", "Kill all enemies", "", "cheat_kill_all", (_, _) =>
            {
                var enemies = LevelManager.Instance.Entities.Where(entity => !(entity is TestCharacter) && !entity.DeathState).ToList();
                if (enemies.Count() == 0)
                {
                    cheatsMenu.Log("There's no enemies to kill", CheatsMenu.LogLevel.Warning);
                    return;
                }
                enemies.ForEach(entity => entity.SetHealth(-1f));
                cheatsMenu.Log($"Successfully killed all {enemies.Count()} enemies", CheatsMenu.LogLevel.Result);
            }),
            new Cheat(name: "Drama cheats", type: Cheat.CheatType.Separator),
            new Cheat("start_cutscene", "Start cutscene", "Starts a cutscene.", "cheat_start_cutscene", (_, input) =>
            {
                var inputArguments = cheatsMenu.ParseCheatArguments(input, 1);
                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var cutsceneName = args[0];
                CutsceneManager.Instance.StartCutscene(cutsceneName);
            }, Cheat.CheatType.Input)
        );
    }
}