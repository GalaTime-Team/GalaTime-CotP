using Godot;
using System;
using Galatime;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace Galatime
{
    public partial class Entity : CharacterBody2D
    {
        [Export] public float speed = 200f;
        public Vector2 velocity = Vector2.Zero;

        public PackedScene damageEffectScene;
        public PackedScene damageAudioScene;
        public PackedScene damageEffect;
        public PackedScene itemPickupScene;
        public PackedScene xpOrbScene;

        public GalatimeElement element = null;
        public CharacterBody2D body = null;
        public AnimationPlayer damageSpritePlayer = null;
        public AudioStreamPlayer2D damageAudioPlayer = null;

        public Timer damageDelay = null;

        public EntityStats Stats = new EntityStats();
        public List<dynamic> lootPool = new List<dynamic>();
        public int droppedXp;
        private bool _deathState;
        public bool DeathState 
        {
            get
            {
                return _deathState;
            }
            private set
            {
                _deathState = value;
            }
        }

        private float health = 0;
        public float Health
        {
            get => health;
            set
            {
                health = value;
                health = Math.Min(Stats[EntityStatType.health].Value, health);
                health = (float)Math.Round(health, 2);
                _healthChangedEvent(health);
            }
        }

        private Vector2 _knockbackVelocity = Vector2.Zero;

        [Signal] public delegate void _onDamageEventHandler();

        public override void _Ready()
        {
            base._Ready();

            damageEffectScene = ResourceLoader.Load<PackedScene>("res://assets/objects/DamageAnimationPlayer.tscn");
            damageAudioScene = ResourceLoader.Load<PackedScene>("res://assets/objects/DamageAudioPlayer.tscn");
            damageEffect = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/damage_effect.tscn");
            itemPickupScene = ResourceLoader.Load<PackedScene>("res://assets/objects/ItemPickup.tscn");
            xpOrbScene = ResourceLoader.Load<PackedScene>("res://assets/objects/ExperienceOrb.tscn");

            damageDelay = new Timer();
            damageDelay.WaitTime = 0.1f;    
            damageDelay.OneShot = true;
            AddChild(damageDelay);
        }

        /// <summary>
        /// Damages and reduces entity health. If health is less than 0 it will call the function <c>_deathEvent()</c>. 
        /// It will also call the <c>_healthChangedEvent()</c> function 
        /// </summary>
        /// <param name="power">Attacker PWR</param>
        /// <param name="attackStat">Attacker ATK</param>
        /// <param name="elemen">Attacker element</param>
        /// <param name="type">Damage type</param>
        /// <param name="knockback">The Power of Knockback</param>
        /// <param name="damageRotation">In radians, will knockback this way. 100 is small knockback</param>
        public void hit(float power, float attackStat, GalatimeElement elemen, DamageType type = DamageType.physical, float knockback = 0f, float damageRotation = 0f)
        {
            GD.Print("Time left: " + damageDelay.TimeLeft);
            if (DeathState || damageDelay.TimeLeft > 0) return;
            damageDelay.Start();
            // Damage animation
            if (damageSpritePlayer == null)
            {
                AnimationPlayer damageSpritePlayerInstance = damageEffectScene.Instantiate<AnimationPlayer>();  
                damageSpritePlayer = damageSpritePlayerInstance;
                body.AddChild(damageSpritePlayerInstance);

                Animation damageAnimation = damageSpritePlayerInstance.GetAnimation("damage");
                damageAnimation.TrackSetPath(0, "Sprite2D:modulate");
            }
                
            if (damageAudioPlayer == null)
            {
                var damageAudioPlayerInstance = damageAudioScene.Instantiate<AudioStreamPlayer2D>();
                damageAudioPlayer = damageAudioPlayerInstance;
                body.AddChild(damageAudioPlayerInstance);
            }

            // Calculating damage
            float damageN = 0;

            if (type == DamageType.physical)
            {
                damageN = attackStat * (power / 10) / Stats[EntityStatType.physicalDefence].Value;
            }
            if (type == DamageType.magical)
            {
                damageN = attackStat * (power / 10) / Stats[EntityStatType.magicalDefence].Value;
            }

            // Calculating weaknesess
            GalatimeElementDamageResult damage = new GalatimeElementDamageResult();
            if (element == null)
            {
                GD.PushWarning("Entity doesn't have a element, default multiplier (1x)");
            } 
            else
            {
                damage = element.getReceivedDamage(elemen, damageN);
                damageN = damage.damage;
                // if (type == DamageType.magical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.magicalDefence.value + " RECEIVER MAGICAL DEFENCE.");
                // if (type == DamageType.physical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.physicalDefence.value + " RECEIVER PHYSICAL DEFENCE.");
            }

            // Damage effect
            Node2D damageEffectInstance = damageEffect.Instantiate() as Node2D;

            damageEffectInstance.Set("number", Math.Round(damageN, 2));
            damageEffectInstance.Set("type", damage.type);
            damageEffectInstance.TopLevel = true;

            damageEffectInstance.GlobalPosition = body.GlobalPosition;
            AddChild(damageEffectInstance);

            if (damageSpritePlayer is AnimationPlayer)
            {
                damageSpritePlayer.Stop();
                damageSpritePlayer.Play("damage");
            }
                
                
            var rand = new Random();
            damageAudioPlayer.PitchScale = (float)(1.1 - rand.NextDouble() / 9);
            damageAudioPlayer.Play();

            // Final
            setKnockback(knockback, damageRotation);

            Health -= damageN;
            if (Health <= 0)
            {
                _deathEvent(damageRotation);
            }
        }

        public void setKnockback(float knockback = 0f, float damageRotation = 0f)
        {
            _knockbackVelocity = Vector2.Right.Rotated(damageRotation) * knockback;
        }

        public override void _PhysicsProcess(double delta)
        {
            _moveProcess();
            _knockbackVelocity = _knockbackVelocity.Lerp(Vector2.Zero, 0.05f);
            velocity += _knockbackVelocity;
            Velocity = velocity;
            MoveAndSlide();
        }

        /// <summary>
        /// Physics process for entity's
        /// </summary>
        public virtual void _moveProcess() {
            velocity = Vector2.Zero;
        }

        /// <summary>
        /// If entity dies event 
        /// if (health <= 0)
        /// </summary>
        public virtual void _deathEvent(float damageRotation = 0f) {
            DeathState = true;
        }

        /// <summary>
        /// If entity changed his health
        /// </summary>
        public virtual void _healthChangedEvent(float health)
        {
        }

        /// <summary>
        /// Drop loot from entity
        /// </summary>
        /// <param name="damageRotation"></param>
        public virtual void _dropLoot(float damageRotation)
        {
            var rnd = new Random();

            for (int i = 0; i < lootPool.Count; i++)
            {
                if (rnd.Next(1, 101) <= lootPool[i].chance)
                {
                    var itemPickup = itemPickupScene.Instantiate() as ItemPickup;
                    var testQuantity = rnd.Next(lootPool[i].min, lootPool[i].max);
                    var spawnVector = new Vector2();
                    spawnVector.X = 200 + rnd.Next(0, 100);
                    spawnVector = spawnVector.Rotated(damageRotation);
                    itemPickup.spawnVelocity = spawnVector;
                    itemPickup.itemId = lootPool[i].id;
                    itemPickup.quantity = testQuantity;
                    itemPickup.GlobalPosition = body.GlobalPosition;

                    GetParent().AddChild(itemPickup);
                }
            }
        }

        public void _dropXp()
        {
            var xpOrb = xpOrbScene.Instantiate<ExperienceOrb>();
            xpOrb.quantity = droppedXp;
            xpOrb.GlobalPosition = body.GlobalPosition;
            GetParent().AddChild(xpOrb);
        }

        public void heal(float amount, int timeToHeal = 0)
        {
            if (damageSpritePlayer == null)
            {
                Node damageSpritePlayerInstance = damageEffectScene.Instantiate();
                body.AddChild(damageSpritePlayerInstance);
                damageSpritePlayer = body.GetNode<AnimationPlayer>("DamageAnimationPlayer");

                Animation damageAnimation = damageSpritePlayer.GetAnimation("heal");
                damageAnimation.TrackSetPath(0, "Sprite2D:modulate");
            }

            if (damageSpritePlayer is AnimationPlayer)
            {
                damageSpritePlayer.Stop();
                damageSpritePlayer.Play("heal");
            }

            Node2D damageEffectInstance = damageEffect.Instantiate() as Node2D;

            damageEffectInstance.Set("number", amount);
            damageEffectInstance.Set("type", "heal");
            damageEffectInstance.TopLevel = true;

            damageEffectInstance.GlobalPosition = body.GlobalPosition;
            AddChild(damageEffectInstance);

            Health += amount;
        }

        public void effect(GalatimeElement type, int duration)
        {

        }
    }
}
