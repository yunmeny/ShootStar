using System;

namespace Core.SystemModules
{
    public enum EncounterMotionStage { Entry, Combat, Exit, Complete }

    public readonly struct EncounterMotionSample
    {
        public readonly float X, Y;
        public readonly EncounterMotionStage Stage;
        public bool CanFire => Stage == EncounterMotionStage.Combat;
        public EncounterMotionSample(float x, float y, EncounterMotionStage stage) { X = x; Y = y; Stage = stage; }
    }

    // Observed horizontal passes in Steredenn: Binary Stars (references in README).
    // Normalized FULL viewport coordinates. Speed is a visual fit, not original source data.
    // The whole phrase is one continuous flight: no arrival easing, combat clamp, or U-turn.
    public static class EncounterMotion
    {
        public const float StartX = 1.2f, EndX = -0.2f;

        public static EncounterMotionSample Sample(EncounterSpawn spawn, float elapsed)
        {
            float age = Math.Max(0f, elapsed - spawn.SpawnAt);
            float x = StartX - age * spawn.Speed;
            var stage = elapsed >= spawn.CompleteAt ? EncounterMotionStage.Complete :
                elapsed >= spawn.ExitAt ? EncounterMotionStage.Exit :
                elapsed >= spawn.EntryAt ? EncounterMotionStage.Combat : EncounterMotionStage.Entry;
            return new EncounterMotionSample(x, spawn.Y, stage);
        }
    }
}
