using Godot;
using System;
using Galatime;
using System.Collections.Generic;

namespace Galatime
{
    public class Entity : Node2D
    {
        [Export] public float speed = 200f;
        public float health = 100f;
        public Vector2 velocity = Vector2.Zero;
        public GalatimeElement element = null;
        public KinematicBody2D body = null;
        public Position2D damageEffectPoint = null;
        public AnimationPlayer damageSpritePlayer = null;

        public EntityStats stats = new EntityStats();
        public List<dynamic> lootPool = new List<dynamic>();

        public Vector2 _knockbackVelocity = Vector2.Zero;

        [Signal] delegate void _onDamage();


        public void hit(float power, float attackStat, GalatimeElement elemen, DamageType type = DamageType.physical, float knockback = 0f, float damageRotation = 0f)
        {
            // Damage animation
            if (damageSpritePlayer == null)
            {
                PackedScene scene = (PackedScene)GD.Load("res://assets/objects/DamageAnimationPlayer.tscn");
                Node damageSpritePlayerInstance = scene.Instance();
                body.AddChild(damageSpritePlayerInstance);
                damageSpritePlayer = body.GetNode<AnimationPlayer>("DamageAnimationPlayer");

                Animation damageAnimation = damageSpritePlayer.GetAnimation("damage");
                damageAnimation.TrackSetPath(0, "Sprite:modulate");
            }

            // Calculating damage
            float damageN = 0;

            if (type == DamageType.physical)
            {
                damageN = attackStat * (power / 10) / stats.physicalDefence;
            }
            if (type == DamageType.magical)
            {
                damageN = attackStat * (power / 10) / stats.magicalDefense;
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
                if (type == DamageType.magical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.magicalDefense + " RECEIVER MAGICAL DEFENCE.");
                if (type == DamageType.physical) GD.Print(damageN + " RECEIVED DAMAGE. " + power + " ATTAKER POWER. " + attackStat + " ATTAKER ATTACK STATS. " + element.name + " RECEIVER ELEMENT NAME. " + elemen.name + " ATTAKER ELEMENT NAME. " + type + " ATTAKER DAMAGE TYPE. " + stats.physicalDefence + " RECEIVER PHYSICAL DEFENCE.");
            }

            // Damage effect
            PackedScene damageEffect = (PackedScene)GD.Load("res://assets/objects/gui/damage_effect.tscn");
            Node2D damageEffectInstance = damageEffect.Instance() as Node2D;

            damageEffectInstance.Set("number", Math.Round(damageN, 2));
            damageEffectInstance.Set("type", damage.type);
            damageEffectInstance.SetAsToplevel(true);

            damageEffectInstance.GlobalPosition = body.GlobalPosition;
            AddChild(damageEffectInstance);

            if (damageSpritePlayer is AnimationPlayer)
            {
                damageSpritePlayer.Stop();
                damageSpritePlayer.Play("damage");
            }

            // Final
            setKnockback(knockback, damageRotation);

            stats.health -= damageN; 
            _healthChangedEvent(health);
            if (stats.health <= 0)
            {
                _deathEvent(damageRotation);
            }
        }

        public void setKnockback(float knockback = 0f, float damageRotation = 0f)
        {
            _knockbackVelocity = Vector2.Left.Rotated(damageRotation) * knockback;
        }

        public override void _PhysicsProcess(float delta)
        {
            _moveProcess();
            _knockbackVelocity = _knockbackVelocity.LinearInterpolate(Vector2.Zero, 0.05f);
            velocity += _knockbackVelocity;
            body.MoveAndSlide(velocity);
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
            QueueFree();
        }

        /// <summary>
        /// If entity changed his health
        /// </summary>
        public virtual void _healthChangedEvent(float health) {

        }

        public virtual void _dropLoot(float damageRotation)
        {
            PackedScene itemPickupScene = GD.Load<PackedScene>("res://assets/objects/ItemPickup.tscn");
            var rnd = new Random();

            for (int i = 0; i < lootPool.Count; i++)
            {
                if (rnd.Next(1, 101) <= lootPool[i].chance)
                {
                    var itemPickup = itemPickupScene.Instance() as ItemPickup;
                    var testQuantity = rnd.Next(lootPool[i].min, lootPool[i].max);
                    var spawnVector = new Vector2();
                    spawnVector.x = -200 + rnd.Next(0, 100);
                    spawnVector = spawnVector.Rotated(damageRotation);
                    itemPickup.spawnVelocity = spawnVector;
                    itemPickup.itemId = lootPool[i].id;
                    itemPickup.quantity = testQuantity;
                    GD.Print("quantity" + testQuantity);
                    itemPickup.GlobalPosition = body.GlobalPosition;

                    GetParent().AddChild(itemPickup);
                }
            }
        }

        public void heal(float amount, int timeToHeal = 0)
        {

        }

        public void effect(GalatimeElement type, int duration)
        {

        }
    }
}
