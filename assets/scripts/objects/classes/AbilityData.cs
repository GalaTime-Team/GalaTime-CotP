using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace Galatime
{
	public struct Costs
	{
		public int Mana;
		public int Stamina;
	}

	public enum AbilityType
	{
		Physical,    // Physical damage ability
		Magical,     // Magical damage ability
		SelfBuff,    // Self-inflicted buff
		TargetBuff,  // Buff applied to target
		Heal,        // Healing ability
		Hybrid       // Combination of types
	}

	public struct StatModifier
	{
		[JsonProperty("stat")]
		public string Stat;  // "speed", "physical_attack", "magical_attack", "defense", etc.
		
		[JsonProperty("value")]
		public float Value;  // Amount to modify (can be negative)
		
		[JsonProperty("duration")]
		public float Duration;  // How long the modifier lasts (0 = permanent)
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class AbilityData
	{
		[JsonProperty("name")]
		/// <summary> The name of the ability. </summary>
		public string Name = "";

		[JsonProperty("id")]
		/// <summary> The unique identificator of the ability. MAKE IT UNIQUE. </summary>
		public string ID = "";

		[JsonProperty("description")]
		/// <summary> The description of the ability. </summary>
		public string Description = "";

		[JsonProperty("element"), JsonConverter(typeof(ElementConverter))]
		/// <summary> The element of the ability. </summary>
		public GalatimeElement Element = new();

		[JsonProperty("costs")]
		/// <summary> The costs of the ability. </summary>
		public Costs Costs = new()
		{
			Mana = 0,
			Stamina = 0
		};

		[JsonProperty("duration")]
		/// <summary> The duration of the ability. </summary>
		public float Duration = 0;

		[JsonProperty("reload")]
		/// <summary> The reloading time of the ability in seconds. </summary>
		public float Reload = 0;

		[JsonProperty("attack")]
		/// <summary> Base attack damage amount. </summary>
		public float Attack = 0;

		[JsonProperty("heal")]
		/// <summary> Healing amount. </summary>
		public float Heal = 0;

		[JsonProperty("ability_type"), JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
		/// <summary> The type of ability (Physical, Magical, SelfBuff, TargetBuff, Heal, Hybrid). </summary>
		public AbilityType Type = AbilityType.Magical;

		[JsonProperty("stat_modifiers")]
		/// <summary> List of stat modifications this ability applies. </summary>
		public List<StatModifier> StatModifiers = new();

		[JsonProperty("range")]
		/// <summary> The range of the ability in pixels. </summary>
		public float Range = 0;

		[JsonProperty("area_of_effect")]
		/// <summary> Area of effect radius. 0 means single target. </summary>
		public float AreaOfEffect = 0;

		[JsonProperty("projectile_speed")]
		/// <summary> Speed of projectile if applicable. </summary>
		public float ProjectileSpeed = 0;

		[JsonProperty("can_crit")]
		/// <summary> Whether this ability can critical hit. </summary>
		public bool CanCrit = true;

		/// <summary> The icon texture of the ability. </summary>
		public Texture2D Icon = null;

		/// <summary> The ability's scene, which will be instantiated when the ability is used. </summary>
		public PackedScene Scene = null;

		[JsonProperty("cost")]
		/// <summary> The cost of the ability in XP. </summary>
		public int CostXP = 0;

		/// <summary> The charges of the ability, which is the number of times the ability can be used. </summary>
		public int Charges = 1;

		private int maxCharges = 1;
		[JsonProperty("charges")]
		/// <summary> The maximum number of charges the ability can have. </summary>
		public int MaxCharges 
		{
			get => maxCharges;
			set
			{
				maxCharges = value;
				Charges = maxCharges;
			}
		}

		/// <summary> The timer used to reload the ability </summary>
		public Timer CooldownTimer = new()
		{
			OneShot = true
		};

		/// <summary> If the ability is reloaded after being used. </summary>
		public bool IsReloaded => CooldownTimer.TimeLeft <= 0 || Charges > 0;

		/// <summary> If the ability is fully reloaded with all charges. </summary> 
		public bool IsFullyReloaded => GodotObject.IsInstanceValid(CooldownTimer) && CooldownTimer.TimeLeft <= 0 || Charges >= MaxCharges;

		public GalatimeAbility Instance;

		private string scenePath = "";
		[JsonProperty("scene")]
		/// <summary> The ability's scene path, which will be instantiated when the ability is used. When assigned, the scene will be loaded in <see cref="Scene"/>. </summary>
		public string ScenePath
		{
			get => scenePath;
			set
			{
				scenePath = value;
				if (scenePath != "")
				{
					Scene = GD.Load<PackedScene>(scenePath);
					Instance = Scene.Instantiate<GalatimeAbility>();
				}
			}
		}

		[JsonProperty("required")]
		/// <summary> The required abilities of the unlocking the ability. </summary>
		public string[] RequiredIDs = new string[0];

		private string _iconPath = "";
		[JsonProperty("icon")]
		/// <summary> The icon texture of the ability. When assigned, the texture will be loaded in <see cref="Icon"/>. </summary>
		public string IconPath
		{
			get => _iconPath;
			set
			{
				_iconPath = value;
				if (_iconPath != "")
				{
					Icon = GD.Load<Texture2D>(_iconPath);
				}
			}
		}

		public bool IsEmpty { get => ID == "" && ScenePath == ""; }

		/// <summary> Create a new instance of the <see cref="AbilityData"/> class. This prevents the item to being wrong referenced. </summary>
		/// <returns> A new instance of the <see cref="AbilityData"/> class. </returns>
		public AbilityData Clone() => (AbilityData)MemberwiseClone();

		public AbilityData()
		{
			if (Reload > 0) CooldownTimer.WaitTime = Reload;
		}

		public static bool operator ==(AbilityData ab1, AbilityData ab2)
		{
			// Handle null cases
			if (ReferenceEquals(ab1, null) && ReferenceEquals(ab2, null)) return true;
			if (ReferenceEquals(ab1, null) || ReferenceEquals(ab2, null)) return false;
			
			return ab1.ID == ab2.ID && ab1.ScenePath == ab2.ScenePath;
		}

		public static bool operator !=(AbilityData ab1, AbilityData ab2) => !(ab1 == ab2);
	}
}
