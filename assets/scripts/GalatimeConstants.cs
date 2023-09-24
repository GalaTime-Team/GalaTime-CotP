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
        /// <summary>
        /// Path3D to Player node path
        /// </summary>
        public const string playerPath = "/root/Node2D/Player";
        /// <summary>
        /// Path3D to Player body node path
        /// </summary>
        public const string playerBodyPath = "/root/Node2D/Player/player_body";
        public const string version = "0.5.0";
        public const string versionDescription = "Upgrade Update\nPREVIEW BUILD";
        public const string savesPath = "user://saves/";
    }
}
