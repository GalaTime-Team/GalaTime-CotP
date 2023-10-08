using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Galatime
{
    public struct Costs {
        public int Mana;
        public int Stamina;
    }

    public class AbilityData
    {
        /// <summary>
        /// The name of the ability.
        /// </summary> 
        public string Name = "";

        /// <summary>
        /// The unique identificator of the ability. MAKE IT UNIQUE.
        /// </summary>
        public string ID = "";

        /// <summary>
        /// The description of the ability.
        /// </summary>
        public string Description = "";

        /// <summary>
        /// The element of the ability.
        /// </summary>
        public GalatimeElement Element = new();

        /// <summary>
        /// The costs of the ability.
        /// </summary>
        public Costs Costs = new() {
            Mana = 0,
            Stamina = 0
        };

        /// <summary>
        /// The duration of the ability.
        /// </summary>
        public float Duration = 0;

        /// <summary>
        /// The reloading time of the ability in seconds.
        /// </summary>
        public float Reload = 0;

        /// <summary>
        /// The icon texture of the ability.
        /// </summary>
        public Texture2D Icon = null;

        /// <summary>
        /// The ability's scene, which will be instantiated when the ability is used.
        /// </summary>
        public PackedScene Scene = null;

        /// <summary>
        /// The cost of the ability in XP. 
        /// </summary>
        public int CostXP = 0;

        /// <summary>
        /// The charges of the ability, which is the number of times the ability can be used.
        /// </summary>
        public int Charges = 1;
        
        /// <summary>
        /// The maximum number of charges the ability can have.
        /// </summary>
        public int MaxCharges = 1;
        
        /// <summary>
        /// The timer used to reload the ability
        /// </summary>
        public Timer CooldownTimer = new() {
            OneShot = true
        };

        /// <summary>
        /// If the ability is reloaded after being used.
        /// </summary>
        public bool IsReloaded => CooldownTimer.TimeLeft <= 0 || Charges > 0;

        /// <summary>
        /// If the ability is fully reloaded with all charges.
        /// </summary> 
        public bool IsFullyReloaded => CooldownTimer.TimeLeft <= 0 && Charges >= MaxCharges;

        public GalatimeAbility Instance;

        private string _scenePath = "";
        /// <summary>
        /// The ability's scene path, which will be instantiated when the ability is used. When assigned, the scene will be loaded in <see cref="Scene"/>.
        /// </summary>
        public string ScenePath {
            get => _scenePath;
            set {
                _scenePath = value;
                if (_scenePath != "") {
                    GD.Print($"Loading scene from ability: {_scenePath}");
                    Scene = GD.Load<PackedScene>(_scenePath);
                    Instance = Scene.Instantiate<GalatimeAbility>();
                }
            }   
        }

        /// <summary>
        /// The required abilities of the unlocking the ability.
        public string[] RequiredIDs = new string[0];

        private string _iconPath = "";
        /// <summary>
        /// The icon texture of the ability. When assigned, the texture will be loaded in <see cref="Icon"/>.
        /// </summary>
        public string IconPath {
            get => _iconPath;
            set {
                _iconPath = value;
                if (_iconPath != "") {
                    GD.Print($"Loading icon from ability: {_iconPath}");
                    Icon = GD.Load<Texture2D>(_iconPath);
                }
            }
        }

        public bool IsEmpty { get => ID == "" && ScenePath == ""; }

        /// <summary>
        /// Create a new instance of the <see cref="AbilityData"/> class. This prevents the item to being wrong referenced.
        /// </summary>
        /// <returns> A new instance of the <see cref="AbilityData"/> class. </returns>
        public AbilityData Clone() {
            return (AbilityData)MemberwiseClone();
        }

        public void PrintAll() {
            // Print the properties of the ability using reflection.
            var output = $"[color=blue]*==========================*[/color]\n\n[color=cyan]Properties of the {Name} ability[/color]:\n\n";
            foreach (var property in GetType().GetProperties()) {
                output += $"{property.Name}: [color=ORANGE_RED]{property.GetValue(this)}[/color]\n";
            }
            output += $"\n[color=cyan]Fields of the {Name} ability fields[/color]:\n\n";
            foreach (var field in GetType().GetFields()) {
                if (field.GetValue(this).GetType() == typeof(Costs)) { 
                    // Get the costs of the ability and print them
                    var costs = (Costs)field.GetValue(this);
                    output += $"{field.Name}: [color=ORANGE_RED]Mana: {costs.Mana}, Stamina: {costs.Stamina}[/color]\n";
                    continue;
                }
                if (field.GetValue(this).GetType() == typeof(GalatimeElement)) {
                    // Get the element of the ability and print it
                    var element = (GalatimeElement)field.GetValue(this);
                    output += $"{field.Name}: [color=ORANGE_RED]{element.Name}[/color]\n";
                    continue;
                }
                output += $"{field.Name}: [color=ORANGE_RED]{field.GetValue(this)}[/color]\n";
            }
            output += "\n[color=blue]*==========================*[/color]";
            GD.PrintRich(output);
        }

        public AbilityData() {
            if (Reload > 0) CooldownTimer.WaitTime = Reload;
        }

        public static bool operator ==(AbilityData ab1, AbilityData ab2) {
            return ab1.ID == ab2.ID && ab1.ScenePath == ab2.ScenePath;
        }

        public static bool operator !=(AbilityData ab1, AbilityData ab2) {
            return !(ab1 == ab2);
        }
    }
}