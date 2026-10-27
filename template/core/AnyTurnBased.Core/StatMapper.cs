using System;

namespace AnyTurnBased.Core
{
    /// <summary>
    /// Maps native values onto a common turn-based scale (docs/08). Native HP is kept 1:1 so writing it
    /// back is exact; attack/defense/speed are normalised against the player's own values so a game
    /// where hits do 300 and one where hits do 3 both give 3-6 hits per kill.
    /// </summary>
    public static class StatMapper
    {
        public const int BaseSpeed = 100;

        /// <param name="nativeDamage">Typical damage per hit in the game's units.</param>
        /// <param name="hitsToKillPeer">Design target: how many basic attacks kill an equal opponent.</param>
        public static Stats FromNative(int hp, int maxHp, float nativeDamage, float nativeArmor,
            float nativeMoveSpeed, float referenceMoveSpeed, int peerHp, float hitsToKillPeer = 4f)
        {
            if (maxHp <= 0) throw new ArgumentException($"maxHp must be > 0 (got {maxHp})");
            if (referenceMoveSpeed <= 0) throw new ArgumentException("referenceMoveSpeed must be > 0");

            // Scale attack so that, before defense, peerHp / attack ≈ hitsToKillPeer.
            float wanted = peerHp / hitsToKillPeer;
            float attack = Math.Max(nativeDamage, wanted);
            return new Stats
            {
                MaxHp = maxHp,
                Hp = Math.Max(0, Math.Min(hp, maxHp)),
                Attack = Math.Max(1, (int)Math.Round(attack)),
                Defense = Math.Max(0, (int)Math.Round(nativeArmor)),
                Speed = Math.Max(10, (int)Math.Round(BaseSpeed * nativeMoveSpeed / referenceMoveSpeed)),
                MaxMp = 20,
                Mp = 20,
            };
        }
    }
}
