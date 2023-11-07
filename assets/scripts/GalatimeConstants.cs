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
        public const string version = "0.7.0";
        public const string versionDescription = "B7\nPREVIEW BUILD";
        public const string savesPath = "user://saves/";
        public const string settingsPath = "user://settings.yml";
        public const string DISCORD_ID = "1071756821158699068";
    }
}
