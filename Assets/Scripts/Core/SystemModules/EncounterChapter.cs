using System;

namespace Core.SystemModules
{
    public enum EncounterKind { DepthFormation, DenseCluster, StaggeredCrossfire, Escort }

    // Data only: no Unity random state, player resources, or reward draws affect a plan.
    public sealed class EncounterSpawn
    {
        public readonly int EnemyIndex;
        public readonly float SpawnAt, X, Y, SpreadX, SpreadY, FireAt, FireEvery;

        public EncounterSpawn(int enemyIndex, float spawnAt, float x, float y,
            float spreadX, float spreadY, float fireAt, float fireEvery)
        {
            EnemyIndex = enemyIndex;
            SpawnAt = spawnAt;
            X = x;
            Y = y;
            SpreadX = spreadX;
            SpreadY = spreadY;
            FireAt = fireAt;
            FireEvery = fireEvery;
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
        public const float EntryDuration = 1.8f;
        public const float ClusterSpreadAt = 5f;
        public const float ClusterSpreadDuration = 2.5f;
        public const float WarningDuration = 0.65f;
        public const float MinVolleyGap = 0.8f;

        public static EncounterPlan[] CreateWave(int wave, int seed, int enemyTypes)
        {
            if (wave < 1 || wave >= BuildTestChapter.BossWave) throw new ArgumentOutOfRangeException(nameof(wave));
            if (enemyTypes < 1) throw new ArgumentOutOfRangeException(nameof(enemyTypes));
            EncounterKind[] kinds;
            switch (wave)
            {
                case 1: kinds = new[] { EncounterKind.DepthFormation }; break;
                case 2: kinds = new[] { EncounterKind.DenseCluster }; break;
                case 3: kinds = new[] { EncounterKind.StaggeredCrossfire }; break;
                case 4: kinds = new[] { EncounterKind.Escort }; break;
                case 5: kinds = new[] { EncounterKind.DepthFormation, EncounterKind.DenseCluster }; break;
                default: kinds = new[] { EncounterKind.DepthFormation, EncounterKind.StaggeredCrossfire,
                    EncounterKind.DenseCluster, EncounterKind.Escort }; break;
            }
            var result = new EncounterPlan[kinds.Length];
            for (int i = 0; i < kinds.Length; i++) result[i] = Create(kinds[i], seed, wave, i, enemyTypes);
            return result;
        }

        private static EncounterPlan Create(EncounterKind kind, int seed, int wave, int phase, int enemyTypes)
        {
            int count = kind == EncounterKind.DenseCluster ? 5 : 4;
            var spawns = new EncounterSpawn[count];
            // An explicit integer hash keeps compositions stable across runtime versions as well.
            uint hash = unchecked((uint)seed * 1664525u + (uint)wave * 1013904223u + (uint)phase * 747796405u);
            int offset = (int)(hash % (uint)enemyTypes);
            for (int i = 0; i < count; i++)
            {
                float spawn = 0f, x = 0f, y = 0f, sx = 0f, sy = 0f;
                float fire = 3.4f + i * 0.9f;
                switch (kind)
                {
                    case EncounterKind.DepthFormation:
                        spawn = (i / 2) * 2.2f + (i % 2) * 0.2f;
                        x = i % 2 == 0 ? 0.28f : 0.72f;
                        y = i / 2 == 0 ? 0.32f : 0.68f;
                        fire = 3.4f + i * 0.9f;
                        break;
                    case EncounterKind.DenseCluster:
                        spawn = i * 0.12f;
                        x = 0.50f + (i % 2) * 0.18f;
                        y = 0.40f + (i / 2) * 0.10f;
                        break;
                    case EncounterKind.StaggeredCrossfire:
                        spawn = i * 0.4f;
                        x = 0.30f + (i / 2) * 0.38f;
                        y = i % 2 == 0 ? 0.25f : 0.75f;
                        // Upper/lower lines alternate, with a central corridor during entry.
                        break;
                    case EncounterKind.Escort:
                        spawn = i * 0.25f;
                        x = i == 0 ? 0.70f : 0.28f + (i == 2 ? 0.10f : 0f);
                        y = i == 0 || i == 2 ? 0.50f : i == 1 ? 0.25f : 0.75f;
                        break;
                }
                sx = kind == EncounterKind.DenseCluster ? 0.26f + (i % 2) * 0.42f : x;
                sy = kind == EncounterKind.DenseCluster ? 0.20f + i * 0.15f : y;
                // Aligned depth pairs share a hull type; other templates mix all existing hulls.
                int type = (offset + (kind == EncounterKind.DepthFormation ? i / 2 : i)) % enemyTypes;
                spawns[i] = new EncounterSpawn(type, spawn, x, y, sx, sy, fire, count * 0.9f + 1.8f);
            }
            return new EncounterPlan(kind, spawns);
        }

        public static string Name(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.DepthFormation: return "纵深编队";
                case EncounterKind.DenseCluster: return "密集集群";
                case EncounterKind.StaggeredCrossfire: return "错位交火";
                default: return "护航编队（普通领机）";
            }
        }

        public static string Hint(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.DepthFormation: return "对齐前后敌机，利用穿透弹道";
                case EncounterKind.DenseCluster: return "集群将在入场后散开，把握导弹或充能齐射窗口";
                case EncounterKind.StaggeredCrossfire: return "上下交替开火，观察炮口预警再换线";
                default: return "后方领机与前排护卫：选择集火目标或先清护卫回能";
            }
        }
    }
}
