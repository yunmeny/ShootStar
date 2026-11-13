using System;

namespace Core.SystemModules
{
    public enum EncounterKind { LaneStream, VerticalColumn, ParallelLanes, MixedSpeedPass }

    // Data only. Coordinates use the complete viewport, including off-screen entry/exit.
    public sealed class EncounterSpawn
    {
        public readonly int EnemyIndex;
        public readonly float SpawnAt, Y, Speed, FireAt, FireEvery;
        public readonly EncounterKind Kind;
        public float EntryAt => SpawnAt + (EncounterMotion.StartX - 1f) / Speed;
        public float ExitAt => SpawnAt + EncounterMotion.StartX / Speed;
        public float CompleteAt => SpawnAt + (EncounterMotion.StartX - EncounterMotion.EndX) / Speed;

        public EncounterSpawn(int enemyIndex, float spawnAt, float y, float speed,
            float fireAt, float fireEvery, EncounterKind kind)
        {
            if (speed <= 0f || float.IsNaN(speed) || float.IsInfinity(speed))
                throw new ArgumentOutOfRangeException(nameof(speed));
            EnemyIndex = enemyIndex;
            SpawnAt = spawnAt;
            Y = y;
            Speed = speed;
            FireAt = fireAt;
            FireEvery = fireEvery;
            Kind = kind;
        }
    }

    public sealed class EncounterPlan
    {
        public readonly EncounterKind Kind;
        public readonly EncounterSpawn[] Spawns;
        public EncounterPlan(EncounterKind kind, EncounterSpawn[] spawns) { Kind = kind; Spawns = spawns; }
    }

    public static class EncounterChapter
    {
        public const int DefaultSeed = 3103;
        public const int MaxActiveEnemies = 6;
        public const int MaxActiveProjectiles = 18;
        public const int MaxVolleyProjectiles = 3;
        public const float MinVolleyGap = 0.8f;
        // Visual fits, in viewport widths/second, NOT extracted original game parameters.
        // Reference: Binary Stars gameplay RQnRHWoOZ2Y, 1:18-1:21 and 4:38-4:49.
        public const float SmallShipSpeed = 0.164f;
        public const float SlowShipSpeed = 0.052f;

        public static EncounterPlan[] CreateWave(int wave, int seed, int enemyTypes)
        {
            if (wave < 1 || wave >= BuildTestChapter.BossWave) throw new ArgumentOutOfRangeException(nameof(wave));
            if (enemyTypes < 1) throw new ArgumentOutOfRangeException(nameof(enemyTypes));
            EncounterKind[] kinds;
            switch (wave)
            {
                case 1: kinds = new[] { EncounterKind.LaneStream }; break;
                case 2: kinds = new[] { EncounterKind.VerticalColumn }; break;
                case 3: kinds = new[] { EncounterKind.ParallelLanes }; break;
                case 4: kinds = new[] { EncounterKind.MixedSpeedPass }; break;
                case 5: kinds = new[] { EncounterKind.LaneStream, EncounterKind.VerticalColumn }; break;
                default: kinds = new[] { EncounterKind.LaneStream, EncounterKind.ParallelLanes,
                    EncounterKind.VerticalColumn, EncounterKind.MixedSpeedPass }; break;
            }
            var result = new EncounterPlan[kinds.Length];
            for (int i = 0; i < kinds.Length; i++) result[i] = Create(kinds[i], seed, wave, i, enemyTypes);
            return result;
        }

        private static EncounterPlan Create(EncounterKind kind, int seed, int wave, int phase, int enemyTypes)
        {
            int count = kind == EncounterKind.MixedSpeedPass ? 5 : 4;
            var spawns = new EncounterSpawn[count];
            uint hash = unchecked((uint)seed * 1664525u + (uint)wave * 1013904223u + (uint)phase * 747796405u);
            int hull = (int)(hash % (uint)enemyTypes);
            for (int i = 0; i < count; i++)
            {
                float spawn, y, speed = SmallShipSpeed;
                int type = hull;
                switch (kind)
                {
                    case EncounterKind.LaneStream:
                        // 1:16-1:21: a horizontal train; no stop, oscillation or return to the right.
                        spawn = i * 0.8f;
                        y = 0.88f;
                        break;
                    case EncounterKind.VerticalColumn:
                        // 4:47-4:49: four visible ships form a vertical column and keep their lanes.
                        spawn = 0f;
                        y = 0.92f - i * 0.14f;
                        break;
                    case EncounterKind.ParallelLanes:
                        // 1:16-1:21: upper and lower ships cross simultaneously, keeping their lanes.
                        spawn = (i / 2) * 0.8f;
                        y = i % 2 == 0 ? 0.88f : 0.18f;
                        break;
                    default:
                        // 4:38-4:49: a slow pass overlaps a later fast column. No invented escort orbit.
                        spawn = i == 0 ? 0f : 18f;
                        y = i == 0 ? 0.75f : 0.92f - (i - 1) * 0.14f;
                        if (i == 0) speed = SlowShipSpeed;
                        else type = (hull + 1) % enemyTypes;
                        break;
                }
                // Keep ShootStar's existing projectile budget and staggered firing policy.
                // Movement is independent of shots, so a denied volley cannot hold or turn a ship.
                float fire = spawn + (EncounterMotion.StartX - 1f) / speed + 0.6f + i * 0.9f;
                spawns[i] = new EncounterSpawn(type, spawn, y, speed, fire, count * 0.9f + 1.8f, kind);
            }
            return new EncounterPlan(kind, spawns);
        }

        public static string Name(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.LaneStream: return "单列穿场";
                case EncounterKind.VerticalColumn: return "四机纵列";
                case EncounterKind.ParallelLanes: return "上下双列";
                default: return "快慢机穿场";
            }
        }

        public static string Hint(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.LaneStream: return "敌机沿同一高度连续向左穿场，提前对齐弹道";
                case EncounterKind.VerticalColumn: return "四机保持纵列穿场，提前选择高度和攻击目标";
                case EncounterKind.ParallelLanes: return "上下两列持续穿场，留意两条航线和左侧来弹";
                default: return "慢机持续向左推进，随后快机入场；留意屏幕两侧的敌机";
            }
        }
    }
}
