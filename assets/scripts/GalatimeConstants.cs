namespace Galatime
{
    public enum StatsType
    {
        PhysicalAttack,
        MagicalAttack,
        PhysicalDefence,
        MagicalDefence,
        Health,
        Mana,
        Stamina,
        Agility,
        StaminaRegen,
        ManaRegen
    }

    public readonly struct GalatimeConstants
    {
        public const string version = "0.6.0";
        public const string versionDescription = "Super combat update\nPREVIEW BUILD";
        public const string savesPath = "user://saves/";
        public const string settingsPath = "user://settings.yml";
    }
}
