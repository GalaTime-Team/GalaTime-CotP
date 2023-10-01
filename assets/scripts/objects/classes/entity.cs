using Godot;
using System;
using System.Collections.Generic;

namespace Galatime
{
    public partial class Entity : CharacterBody2D
    {
        #region Variables
        [Export] public float Speed = 200f;
        public EntityStats Stats = new();
        public List<dynamic> LootPool = new();
        public int DroppedXp;
        private Vector2 KnockbackVelocity = Vector2.Zero;
        #endregion

        #region Scenes
        public PackedScene DamageEffectScene;
        public PackedScene DamageAnimationPlayerScene;
        public PackedScene DamageAudioScene;
        public PackedScene ItemPickupScene;
        public PackedScene XpOrbScene;
        #endregion

        #region Nodes
        public GalatimeElement Element = null;
        public CharacterBody2D Body = null;
        public AnimationPlayer DamageSpritePlayer = null;
        public AudioStreamPlayer2D DamageAudioPlayer = null;
        public Timer DamageDelay = null;
        #endregion

        #region Properties
        /// <summary> If the entity is dead.  </summary>
        public bool DeathState { get; private set; }

        private float health = 0;
        /// <summary> Entity health, will be between 0 and Health stat. Fires the <see cref="_healthChangedEvent"/> every time if health is changed. </summary>
        public float Health
        {
            get => health;
            set
            {
                health = value;
                health = Math.Min(Stats[EntityStatType.Health].Value, health);
                health = (float)Math.Round(health, 2);
                HealthChangedEvent(health);
            }
        }
        #endregion

        #region Events
        public Action OnDeath;
        #endregion

        public override void _Ready()
        {
            base._Ready();
            LoadScenes();

            // Creates damage delay to prevent to many damage in a short time.
            DamageDelay = new Timer
            {
                Name = "DamageDelay",
                WaitTime = 0.1f,
                OneShot = true
            };
            AddChild(DamageDelay);
        }

        /// <summary> Loads the scenes of the entity. </summary>
        private void LoadScenes()
        {
            DamageAnimationPlayerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/DamageAnimationPlayer.tscn");
            DamageAudioScene = ResourceLoader.Load<PackedScene>("res://assets/objects/DamageAudioPlayer.tscn");
            DamageEffectScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/DamageEffect.tscn");
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
        public void TakeDamage(float power, float attackStat, GalatimeElement element, DamageType type = DamageType.physical, float knockback = 0f, float damageRotation = 0f)
        {
            // Checking if entity is delayed
            if (DeathState || DamageDelay.TimeLeft > 0) return;
            DamageDelay.Start();

            InstantiateFirstTime();

            // Calculating damage
            float damageN = 0;
            var damageMultiplier = attackStat * (power / 10);
            // Calculating damage based on type.
            if (type == DamageType.physical) damageN = damageMultiplier / Stats[EntityStatType.PhysicalDefense].Value;
            if (type == DamageType.magical) damageN = damageMultiplier / Stats[EntityStatType.MagicalDefense].Value;

            // Calculating weaknesses
            GalatimeElementDamageResult damageResult = new();
            if (Element == null) GD.PushWarning("Entity doesn't have a element, default multiplier (1x)");
            else
            {
                damageResult = Element.GetReceivedDamage(element, damageN);
                damageN = (float)Math.Round(damageResult.Damage, 1);
                // if (type == DamageType.magical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.magicalDefence.value + " RECEIVER MAGICAL DEFENCE.");
                // if (type == DamageType.physical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.physicalDefence.value + " RECEIVER PHYSICAL DEFENCE.");
            }

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
            Health -= damageN;
            if (Health <= 0) _DeathEvent(damageRotation);
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
        }

        /// <summary>
        /// Set knockback for entity by rotation and knockback (Applying movement impulse). 
        /// </summary>
        /// <param name="knockback">How stronger is knockback. 100 is a small knockback</param>
        /// <param name="damageRotation">In radians, will knockback this way. </param>
        public void SetKnockback(float knockback = 0f, float damageRotation = 0f)
        {
            KnockbackVelocity = Vector2.Right.Rotated(damageRotation) * knockback;
        }

        public override void _PhysicsProcess(double delta)
        {
            _MoveProcess();
            KnockbackVelocity = KnockbackVelocity.Lerp(Vector2.Zero, 0.05f);
            Velocity += KnockbackVelocity;
            MoveAndSlide();
        }

        /// <summary> Physics process for entity's </summary>
        public virtual void _MoveProcess()
        {
            Velocity = Vector2.Zero;
        }

        /// <summary> If entity dies event </summary>
        public virtual void _DeathEvent(float damageRotation = 0f)
        {
            DeathState = true;
            OnDeath?.Invoke();
        }

        /// <summary> If entity changed his health </summary>
        public virtual void HealthChangedEvent(float health)
        {
        }

        /// <summary> Drop loot from entity. </summary>
        /// <param name="damageRotation">The rotation to drop loot.</param>
        public virtual void DropLoot(float damageRotation)
        {
            var rnd = new Random();

            // Inserting loot pool to drop.
            for (int i = 0; i < LootPool.Count; i++)
            {
                // Calculating chance to drop.
                if (rnd.Next(1, 101) <= LootPool[i].chance)
                {
                    // Instantiating item pickup to drop.
                    var itemPickup = ItemPickupScene.Instantiate<ItemPickup>();

                    // Setting item pickup random values (Quantity and spawn velocity).
                    var quantity = rnd.Next(LootPool[i].min, LootPool[i].max);
                    var spawnVector = new Vector2 { X = 200 + rnd.Next(0, 100) };
                    spawnVector = spawnVector.Rotated(damageRotation);

                    // Setting item pickup values.
                    itemPickup.SpawnVelocity = spawnVector;
                    itemPickup.ItemId = LootPool[i].id;
                    itemPickup.Quantity = quantity;
                    itemPickup.GlobalPosition = Body.GlobalPosition;

                    // Adding item pickup to the scene.
                    GetParent().AddChild(itemPickup);
                }
            }
        }


        /// <summary> Drop xp from entity based on <see cref="DroppedXp"/>, so <see cref="DroppedXp"/> will determine how much xp will be dropped. </summary> 
        public void DropXp()
        {
            var xpOrb = XpOrbScene.Instantiate<ExperienceOrb>();
            xpOrb.Quantity = DroppedXp;
            xpOrb.GlobalPosition = Body.GlobalPosition;
            GetParent().AddChild(xpOrb);
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

            // Adding health to the entity.
            Health += amount;
        }

        /// <summary>
        /// Spawns damage effect.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        public void SpawnDamageEffect(float amount, DamageDifferenceType type) {
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
    }
}
