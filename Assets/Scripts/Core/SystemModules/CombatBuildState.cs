using System;

namespace Core.SystemModules
{
    // Transient combat resources share the scene lifetime of RunUpgradeState.
    public sealed class CombatBuildState
    {
        public const int MaxCharge = 100;
        public const float BoostDuration = 2.5f;
        public const int MaxSecondaryBlasts = 4;
        public float BoostRemaining { get; private set; }
        public int Charge { get; private set; }
        public int ReclaimerProgress { get; private set; }
        public float FireRateMultiplier => BoostRemaining > 0f ? 1.5f : 1f;

        public void Tick(float deltaTime) => BoostRemaining = Math.Max(0f, BoostRemaining - Math.Max(0f, deltaTime));
        public void GrantBoost() => BoostRemaining = BoostDuration;
        public void AddCharge(int spent) => Charge = Math.Min(MaxCharge, Charge + Math.Max(0, spent));
        public float ConsumeVolley()
        {
            float multiplier = 1f + 2f * Charge / MaxCharge;
            Charge = 0;
            return multiplier;
        }

        public int RecordMissileKills(int kills)
        {
            int total = ReclaimerProgress + Math.Max(0, kills);
            ReclaimerProgress = total % 3;
            return Math.Min(1, total / 3);
        }
    }
}
