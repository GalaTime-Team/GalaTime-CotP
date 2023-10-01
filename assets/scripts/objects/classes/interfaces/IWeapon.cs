using Galatime;

namespace Galatime.Interfaces;

/// <summary> Interface for weapons which contain all necessary information for weapon. </summary>
public interface IWeapon
{
    public float Power { get; set; }
    public float Cooldown { get; set; }
    public void Attack(HumanoidCharacter p);
}