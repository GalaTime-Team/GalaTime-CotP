using Galatime.Global;
using Galatime.Helpers;
using Godot;
using System;

namespace Galatime;

public partial class Entity : CharacterBody2D
{
	#region Variables
	/// <summary> Stats, which are applied to entity. </summary>
	[Export] public EntityStats Stats { get; set; }
	
	[Export] public GalatimeElement Element { get; set; }
	[Export] public Teams Team;
	/// <summary> How many XP is dropped from entity when it's died </summary>
	[Export] public int DroppedXp;
	/// <summary> Speed of the entity moving. </summary>
	[Export] public float Speed = 200f;
	/// <summary> How long the entity will be valid after death. 0 means never. </summary>
	[Export] public float Timeout = 3f;
	/// <summary> If the entity is invincible, meaning it won't take any damage. It can be killed if SetHealth is called with negative value. </summary>
	[Export] public bool Invincible = false;
	
	/// <summary> Ability IDs to load automatically (e.g., "fireball", "firebullet"). Max 3 abilities. </summary>
	[Export] public Godot.Collections.Array<string> DefaultAbilityIds { get; set; } = new();
	
	/// <summary> AI rules to configure behavior automatically. Can be set up in Godot editor. </summary>
	[Export] public Godot.Collections.Array<AI.Controller.AIRuleData> AIRules { get; set; } = new();
	
	/// <summary> Whether to automatically setup AI from AIRules on ready. </summary>
	[Export] public bool AutoSetupAI { get; set; } = true;
	
	/// <summary> Whether to enable debug mode for AI Controller. </summary>
	[Export] public bool AIDebugMode { get; set; } = false;
	
	private Vector2 KnockbackVelocity = Vector2.Zero;
	public bool CanMove = true;
	/// <summary> If entity can do AI. It means it will be processed by <see cref="_AIProcess"/> </summary>
	public bool DisableAI;
	/// <summary> If entity should be ignored by AI. </summary>
	public bool AIIgnore;
	/// <summary> The list of abilities available to this entity (up to 3 ranged attacks). </summary>
	public System.Collections.Generic.List<AbilityData> Abilities = new();
	/// <summary> Custom AI behaviors that can be assigned to this entity. </summary>
	public System.Collections.Generic.List<System.Action<double>> AIBehaviors = new();
	/// <summary> The AI controller for this entity (created automatically if AIRules are set). </summary>
	public AI.Controller.AIController AIController { get; private set; }
	#endregion

	#region Scenes
	public PackedScene DamageEffectScene;
	public PackedScene DamageAnimationPlayerScene;
	public PackedScene DamageAudioScene;

	public PackedScene HealAudioScene;

	public PackedScene ItemPickupScene;
	public PackedScene XpOrbScene;
	#endregion

	#region Nodes
	[Export] public CharacterBody2D Body;
	public AnimationPlayer DamageSpritePlayer = null;
	public AudioStreamPlayer2D DamageAudioPlayer = null;
	public AudioStreamPlayer2D HealAudioPlayer = null;
	public Timer DamageDelay = null;
	public Timer DeathTimer = null;
	#endregion

	#region Properties
	/// <summary> If the entity is dead.  </summary>
	public bool DeathState { get; private set; }

	public bool ForceDeathState(bool state) => DeathState = state;

	private float health = 0;
	/// <summary> Entity health, will be between 0 and Health stat. Fires the <see cref="_healthChangedEvent"/> every time if health is changed. </summary>
	public float Health
	{
		get => health;
		set => SetHealth(value);
	}
	public void SetHealth(float value, float damageRotation = 0f)
	{
		if (Invincible && value < 0) return;
		health = Math.Clamp((float)Math.Round(value, 2), 0, Stats[EntityStatType.Health].Value);
		HealthChangedEvent(health);
		OnHealthChanged?.Invoke(health);
		if (health <= 0)
		{
			DeathState = true;
			_DeathEvent(damageRotation);
			OnDeath?.Invoke();
			DeathTimer?.Start();
		}
	}
	#endregion

	#region Events
	public Action OnDeath;
	public Action OnRevived;
	public Action<float> OnHealthChanged;
	#endregion

	// public Entity(EntityStats stats = null) => (Stats) = (stats);

	public override void _Ready()
	{
		LoadScenes();

		Health = Stats[EntityStatType.Health].Value;

		// Creates damage delay to prevent to many damage in a short time.
		DamageDelay = new Timer
		{
			Name = "DamageDelay",
			WaitTime = 0.1f,
			OneShot = true
		};
		AddChild(DamageDelay);

		// If timeout is set, creates death timer to destroy the entity.
		if (Timeout > 0)
		{
			DeathTimer = new Timer
			{
				Name = "DeathTimer",
				WaitTime = Timeout,
				OneShot = true
			};
			AddChild(DeathTimer);
			DeathTimer.Timeout += () => QueueFree();
		}

		// Ensure Stats dictionary is initialized from fixed properties
		if (Stats != null && Stats.Count == 0)
		{
			Stats.InitializeStats();
		}

		// Automatically load abilities from exported IDs
		LoadDefaultAbilities();

		// Automatically setup AI from exported rules
		if (AutoSetupAI)
		{
			SetupAIFromRules();
		}

		// Needed to register entity to level manager.
		LevelManager.Instance.RegisterEntity(this);
	}

	/// <summary> Loads the scenes of the entity. </summary>
	private void LoadScenes()
	{
		DamageAnimationPlayerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/DamageAnimationPlayer.tscn");
		DamageAudioScene = ResourceLoader.Load<PackedScene>("res://assets/objects/DamageAudioPlayer.tscn");
		DamageEffectScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/DamageEffect.tscn");

		HealAudioScene = ResourceLoader.Load<PackedScene>("res://assets/objects/entity/HealAudioPlayer.tscn");

		ItemPickupScene = ResourceLoader.Load<PackedScene>("res://assets/objects/ItemPickup.tscn");
		XpOrbScene = ResourceLoader.Load<PackedScene>("res://assets/objects/ExperienceOrb.tscn");
	}

	/// <summary>
	/// Damages and reduces entity health. If health is less than 0 it will call the function <see cref="OnDeath"/> and fire the <see cref="OnDeath"/> event. 
	/// It will also call the <c>_healthChangedEvent()</c> function 
	/// </summary> 
	/// <param name="power">Attacker PWR</param>
	/// <param name="attackStat">Attacker ATK</param>
	/// <param name="element">Attacker element</param>
	/// <param name="type">Damage type</param>
	/// <param name="knockback">The Power of Knockback</param>
	/// <param name="damageRotation">In radians, will knockback this way. 100 is a small knockback</param>
	public void TakeDamage(float power, float attackStat, GalatimeElement element, DamageType type = DamageType.Physical, float knockback = 0f, float damageRotation = 0f)
	{
		// Checking if entity is delayed or invincible.
		if (DeathState || DamageDelay.TimeLeft > 0 || Invincible) return;
		DamageDelay.Start();

		InstantiateFirstTime();

		// Calculating damage.
		float damageN = 0;
		var damageMultiplier = attackStat * (power / 10);
		// Calculating damage based on type.
		if (type == DamageType.Physical) damageN = damageMultiplier / Stats[EntityStatType.PhysicalDefense].Value;
		if (type == DamageType.Magical) damageN = damageMultiplier / Stats[EntityStatType.MagicalDefense].Value;

		// Calculating weaknesses.
		GalatimeElementDamageResult damageResult = new();
		if (Element == null) GD.PushWarning("Entity doesn't have a element, default multiplier (1x)");
		else
		{
			damageResult = Element.GetReceivedDamage(element, damageN);
			damageN = (float)Math.Round(damageResult.Damage, 1);
			// if (type == DamageType.magical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.magicalDefence.value + " RECEIVER MAGICAL DEFENCE.");
			// if (type == DamageType.physical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.physicalDefence.value + " RECEIVER PHYSICAL DEFENCE.");
		}

		// Round damageN to the nearest integer and ensure it's at least 1.
		damageN = Math.Max((int)Math.Round(damageN), 1);

		SpawnDamageEffect(damageN, damageResult.Type);

		if (DamageSpritePlayer is not null)
		{
			DamageSpritePlayer.Stop();
			DamageSpritePlayer.Play("damage");
		}

		// Playing damage audio with random pitch.
		var rand = new Random();
		DamageAudioPlayer.PitchScale = (float)(1.1 - rand.NextDouble() / 9);
		DamageAudioPlayer.Play();

		// Final, setting knockback and rotation of the source of the damage.
		SetKnockback(knockback, damageRotation);

		// Reducing health.
		SetHealth(Health - damageN, damageRotation);
	}

	/// <summary>
	/// Restores an entity to life with full health. Used when the entity should be brought back from death.
	/// Triggers an OnRevived event to allow other systems to respond to the revival.
	/// </summary>
	public void Revive()
	{
		if (!DeathState) return;

		DeathState = false;
		Heal(Stats[EntityStatType.Health].Value); // Restores the entity's health to full.

		OnRevived?.Invoke(); // Notify any listeners that the entity has been revived.
	}

	/// <summary> Instantiates all nodes of the entity if they don't exist. </summary>
	private void InstantiateFirstTime()
	{
		// Adding damage sprite animation player if it doesn't exist
		if (DamageSpritePlayer == null)
		{
			// Instantiate damage animation player to add red effect when damage is taken.
			AnimationPlayer damageSpritePlayerInstance = DamageAnimationPlayerScene.Instantiate<AnimationPlayer>();
			DamageSpritePlayer = damageSpritePlayerInstance;
			Body.AddChild(damageSpritePlayerInstance);

			// We apply red effect to animation track and set its path.
			Godot.Animation damageAnimation = damageSpritePlayerInstance.GetAnimation("damage");
			damageAnimation.TrackSetPath(0, "Sprite2D:modulate");
		}

		// Adding damage audio player if it doesn't exist
		if (DamageAudioPlayer == null)
		{
			// Instantiate damage audio player.
			var damageAudioPlayerInstance = DamageAudioScene.Instantiate<AudioStreamPlayer2D>();
			DamageAudioPlayer = damageAudioPlayerInstance;
			Body.AddChild(damageAudioPlayerInstance);
		}

		if (HealAudioPlayer == null)
		{
			var healAudioPlayerInstance = HealAudioScene.Instantiate<AudioStreamPlayer2D>();
			HealAudioPlayer = healAudioPlayerInstance;
			Body.AddChild(healAudioPlayerInstance);
		}
	}

	/// <summary>
	/// Set knockback for entity by rotation and knockback (Applying movement impulse).
	/// </summary>
	/// <param name="knockback">How stronger is knockback. 100 is a small knockback</param>
	/// <param name="damageRotation">In radians, will knockback this way. </param>
	public void SetKnockback(float knockback = 0f, float damageRotation = 0f)
	{
		KnockbackVelocity += Vector2.Right.Rotated(damageRotation) * Math.Max(knockback - Stats[EntityStatType.KnockbackResistance].Value, 0);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!CanMove || DeathState)
		{
			Body.Velocity = Vector2.Zero;
		}

		_MoveProcess(delta);
		// AI
		if (!DisableAI) _AIProcess(delta);

		KnockbackVelocity = KnockbackVelocity.Lerp(Vector2.Zero, 0.05f);
		if (Body is not null)
		{
			Body.Velocity += KnockbackVelocity;
			Body.MoveAndSlide();
		}
	}

	/// <summary> AI process for entity's, called by <see cref="_PhysicsProcess"/>. It not called if <see cref="DisableAI"/> is true. </summary>
	public virtual void _AIProcess(double delta)
	{
		// Execute all custom AI behaviors
		foreach (var behavior in AIBehaviors)
		{
			behavior?.Invoke(delta);
		}
	}

	/// <summary> Physics process for entity's </summary>
	public virtual void _MoveProcess(double delta)
	{
	}

	/// <summary> If entity dies event </summary>
	public virtual void _DeathEvent(float damageRotation = 0f)
	{
	}

	/// <summary> If entity changed his health </summary>
	public virtual void HealthChangedEvent(float health)
	{
	}

	/// <summary> Drop loot from entity. </summary>
	/// <param name="damageRotation">The rotation to drop loot.</param>
	// public virtual void DropLoot(float damageRotation)
	// {
	//     var rnd = new Random();

	//     // Inserting loot pool to drop.
	//     for (int i = 0; i < LootPool.Count; i++)
	//     {
	//         // Calculating chance to drop.
	//         if (rnd.Next(1, 101) <= LootPool[i].chance)
	//         {
	//             // Instantiating item pickup to drop.
	//             var itemPickup = ItemPickupScene.Instantiate<ItemPickup>();

	//             // Setting item pickup random values (Quantity and spawn velocity).
	//             var quantity = rnd.Next(LootPool[i].min, LootPool[i].max);
	//             var spawnVector = new Vector2 { X = 200 + rnd.Next(0, 100) };
	//             spawnVector = spawnVector.Rotated(damageRotation);

	//             // Setting item pickup values.
	//             itemPickup.SpawnVelocity = spawnVector;
	//             itemPickup.ItemId = LootPool[i].id;
	//             itemPickup.Quantity = quantity;
	//             itemPickup.GlobalPosition = Body.GlobalPosition;

	//             // Adding item pickup to the scene.
	//             GetParent().AddChild(itemPickup);
	//         }
	//     }
	// }


	/// <summary> Drop xp from entity based on <see cref="DroppedXp"/>, so <see cref="DroppedXp"/> will determine how much xp will be dropped. </summary> 
	public void DropXp()
	{
		var xpOrb = XpOrbScene.Instantiate<ExperienceOrb>();
		Callable.From(() =>
		{
			xpOrb.Quantity = DroppedXp;
			GetParent().AddChild(xpOrb);
			xpOrb.GlobalPosition = Body.GlobalPosition;
		}).CallDeferred();
	}

	public void Heal(float amount, int timeToHeal = 0)
	{
		InstantiateFirstTime();

		if (DamageSpritePlayer is not null)
		{
			DamageSpritePlayer.Stop();
			DamageSpritePlayer.Play("heal");
		}

		SpawnDamageEffect(amount, DamageDifferenceType.heal);

		HealAudioPlayer?.Play();

		// Adding health to the entity.
		Health += amount;
	}

	/// <summary>
	/// Spawns damage effect.
	/// </summary>
	/// <param name="amount"></param>
	/// <param name="type"></param>
	public void SpawnDamageEffect(float amount, DamageDifferenceType type)
	{
		// If damage indicator is disabled, don't show the effect.
		if (SettingsGlobals.Settings.Misc.DisableDamageIndicator) return;

		var damageEffectInstance = DamageEffectScene.Instantiate<DamageEffect>();

		// Setting damage effect and his properties
		damageEffectInstance.Number = amount;
		damageEffectInstance.Type = type;
		damageEffectInstance.TopLevel = true;

		// Adding damage effect to entity
		damageEffectInstance.GlobalPosition = Body.GlobalPosition;
		AddChild(damageEffectInstance);
	}

	public void Effect(GalatimeElement type, int duration)
	{
		// TODO: Implement effect
	}

	#region Ability System
	/// <summary> Adds an ability to the entity at the specified index. </summary>
	/// <param name="ability">The ability data to add.</param>
	/// <param name="index">The index where to add the ability (0-2 for ranged attacks).</param>
	public virtual void AddAbility(AbilityData ability, int index)
	{
		// Ensure the abilities list has enough space
		while (Abilities.Count <= index) Abilities.Add(new AbilityData());
		
		Abilities[index] = ability;
		
		// Setup cooldown timer if the ability has a reload time
		if (ability.Reload > 0)
		{
			ref Timer cooldownTimer = ref Abilities[index].CooldownTimer;
			
			// Clean up previous timer if it exists
			if (cooldownTimer != null && GodotObject.IsInstanceValid(cooldownTimer))
			{
				cooldownTimer.Stop();
				cooldownTimer.QueueFree();
				cooldownTimer = null;
			}
			
			// Create new cooldown timer
			cooldownTimer = new Timer
			{
				Name = $"{ability.Name}CooldownTimer",
				WaitTime = ability.Reload,
				OneShot = true
			};
			cooldownTimer.Timeout += () => OnAbilityCooldownComplete(index);
			AddChild(cooldownTimer);
			
			// Start timer if ability is not fully charged
			if (Abilities[index].Charges < Abilities[index].MaxCharges)
			{
				cooldownTimer.Start();
			}
		}
	}

	/// <summary> Called when an ability cooldown completes. </summary>
	protected virtual void OnAbilityCooldownComplete(int index)
	{
		var ability = Abilities[index];
		if (ability.Charges < ability.MaxCharges)
		{
			ability.Charges++;
			if (ability.Charges < ability.MaxCharges)
			{
				ability.CooldownTimer.Start();
			}
		}
	}

	/// <summary> Uses an ability at the specified index. </summary>
	/// <param name="index">The index of the ability to use (0-2).</param>
	/// <returns>True if the ability was successfully used, false otherwise.</returns>
	public virtual bool UseAbility(int index)
	{
		// Check if index is valid
		if (index < 0 || index >= Abilities.Count) return false;
		
		var ability = Abilities[index];
		
		// Check if ability can be used
		if (ability.IsEmpty || !ability.IsReloaded || ability.Charges <= 0) return false;
		
		// Load and execute the ability
		var abilityScene = ResourceLoader.Load<PackedScene>(ability.ScenePath);
		var abilityInstance = abilityScene.Instantiate<GalatimeAbility>();
		abilityInstance.Data = ability;
		
		// Add the ability to the scene and execute it
		GetParent().AddChild(abilityInstance);
		abilityInstance.Execute(this);
		
		// Start cooldown and reduce charges
		ability.CooldownTimer.Stop();
		ability.CooldownTimer.Start();
		ability.Charges--;
		
		return true;
	}

	/// <summary> Removes an ability at the specified index. </summary>
	public virtual void RemoveAbility(int index)
	{
		if (index < 0 || index >= Abilities.Count) return;
		
		var ability = Abilities[index];
		if (ability.CooldownTimer != null && GodotObject.IsInstanceValid(ability.CooldownTimer))
		{
			ability.CooldownTimer.Stop();
			ability.CooldownTimer.QueueFree();
		}
		
		Abilities[index] = new AbilityData();
	}
	#endregion

	#region AI System
	/// <summary> Adds a custom AI behavior to this entity. </summary>
	/// <param name="behavior">The AI behavior action that takes delta time as parameter.</param>
	public void AddAIBehavior(System.Action<double> behavior)
	{
		if (!AIBehaviors.Contains(behavior))
		{
			AIBehaviors.Add(behavior);
		}
	}

	/// <summary> Removes a custom AI behavior from this entity. </summary>
	public void RemoveAIBehavior(System.Action<double> behavior)
	{
		AIBehaviors.Remove(behavior);
	}

	/// <summary> Clears all custom AI behaviors. </summary>
	public void ClearAIBehaviors()
	{
		AIBehaviors.Clear();
	}
	
	/// <summary>
	/// Loads abilities from DefaultAbilityIds array automatically.
	/// Called during _Ready() if abilities are specified in the editor.
	/// </summary>
	private void LoadDefaultAbilities()
	{
		if (DefaultAbilityIds == null || DefaultAbilityIds.Count == 0) return;
		
		for (int i = 0; i < System.Math.Min(DefaultAbilityIds.Count, 3); i++)
		{
			var abilityId = DefaultAbilityIds[i];
			if (string.IsNullOrEmpty(abilityId)) continue;
			
			var ability = GalatimeGlobals.GetAbilityById(abilityId);
			if (ability != null)
			{
				AddAbility(ability, i);
			}
			else
			{
				GD.PushWarning($"Entity {Name}: Failed to load ability '{abilityId}'");
			}
		}
	}
	
	/// <summary>
	/// Sets up AI Controller from exported AIRules.
	/// Called during _Ready() if AutoSetupAI is true and AIRules are defined.
	/// </summary>
	private void SetupAIFromRules()
	{
		if (AIRules == null || AIRules.Count == 0) return;
		
		// Create AI Controller
		AIController = new AI.Controller.AIController();
		AIController.Entity = this;
		AIController.DebugMode = AIDebugMode;
		AddChild(AIController);
		
		// Add rules from exported data
		foreach (var ruleData in AIRules)
		{
			if (ruleData == null) continue;
			
			var rule = AI.Controller.AIRuleFactory.CreateRule(ruleData, this);
			if (rule != null)
			{
				AIController.AddRule(rule);
			}
		}
		
		// Integrate with entity AI system
		AddAIBehavior((delta) => AIController.Process(delta));
	}
	#endregion
}
