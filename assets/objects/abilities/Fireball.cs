using Galatime.Damage;
using Godot;

namespace Galatime
{
    public partial class Fireball : GalatimeAbility
    {
        private BaseFireball BaseFireball;

        public override void _Ready()
        {
            BaseFireball = GetNode<BaseFireball>("BaseFireball");
        }

        public override void Execute(HumanoidCharacter p)
        {
            var magicalAttack = p.Stats[EntityStatType.MagicalAttack].Value;
            BaseFireball.GlobalPosition = p.Weapon.GlobalPosition;
            BaseFireball.Proj.Duration = Data.Duration;
            BaseFireball.Launch(p.Weapon.Rotation, magicalAttack);
        }
    }
}
