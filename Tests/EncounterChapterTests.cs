using System;
using Core.SystemModules;

// Standalone: compile with EncounterChapter.cs and BuildTestChapter.cs.
public static class EncounterChapterTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Main()
    {
        var expected = new[] { EncounterKind.DepthFormation, EncounterKind.DenseCluster,
            EncounterKind.StaggeredCrossfire, EncounterKind.Escort };
        for (int i = 0; i < expected.Length; i++)
        {
            var wave = EncounterChapter.CreateWave(i + 1, EncounterChapter.DefaultSeed, 3);
            Check(wave.Length == 1 && wave[0].Kind == expected[i], "First four waves teach one template each");
        }
        var fifth = EncounterChapter.CreateWave(5, EncounterChapter.DefaultSeed, 3);
        Check(fifth.Length == 2 && fifth[0].Kind == expected[0] && fifth[1].Kind == expected[1], "Wave five combines depth and cluster");
        var sixth = EncounterChapter.CreateWave(6, EncounterChapter.DefaultSeed, 3);
        Check(sixth.Length == 4, "Final regular wave combines all templates");
        foreach (var kind in expected) Check(Array.Exists(sixth, plan => plan.Kind == kind), "Final wave covers every template");

        foreach (int seed in new[] { int.MinValue, -1, 0, 1, 3103, int.MaxValue })
        for (int types = 1; types <= 3; types++)
        for (int wave = 1; wave <= 6; wave++)
        {
            var a = EncounterChapter.CreateWave(wave, seed, types);
            // Reward/random draws, earlier waves, and build selection cannot change the plan.
            EncounterChapter.CreateWave(6, seed + (seed == int.MaxValue ? -1 : 1), types);
            var b = EncounterChapter.CreateWave(wave, seed, types);
            Check(a.Length == b.Length, "Recreating a seed preserves phase count");
            for (int phase = 0; phase < a.Length; phase++)
            {
                var plan = a[phase];
                Check(plan.Spawns.Length <= EncounterChapter.MaxActiveEnemies, "Each phase respects enemy budget even if none die");
                Check(plan.Kind == b[phase].Kind && plan.Spawns.Length == b[phase].Spawns.Length, "Template and count reproduce");
                float lastSpawn = -1f;
                for (int i = 0; i < plan.Spawns.Length; i++)
                {
                    var spawn = plan.Spawns[i];
                    var copy = b[phase].Spawns[i];
                    Check(spawn.EnemyIndex >= 0 && spawn.EnemyIndex < types, "Composition uses available prefabs");
                    Check(spawn.SpawnAt >= lastSpawn, "Entry schedule stays ordered");
                    lastSpawn = spawn.SpawnAt;
                    Check(spawn.X >= 0.2f && spawn.X <= 0.75f && spawn.SpreadX >= 0.2f && spawn.SpreadX <= 0.75f,
                        "Entry destinations and scatter preserve the player's half");
                    Check(spawn.Y >= 0.2f && spawn.Y <= 0.8f && spawn.SpreadY >= 0.2f && spawn.SpreadY <= 0.8f,
                        "Ships keep clear of vertical boundaries");
                    Check(spawn.FireAt - EncounterChapter.WarningDuration >= spawn.SpawnAt + EncounterChapter.EntryDuration,
                        "First warning follows complete entry; no unannounced entry fire");
                    Check(spawn.FireEvery >= plan.Spawns.Length * EncounterChapter.MinVolleyGap,
                        "Schedule has room for each volley without crowding the shared budget");
                    Check(spawn.EnemyIndex == copy.EnemyIndex && spawn.SpawnAt == copy.SpawnAt && spawn.X == copy.X &&
                        spawn.Y == copy.Y && spawn.SpreadX == copy.SpreadX && spawn.SpreadY == copy.SpreadY &&
                        spawn.FireAt == copy.FireAt && spawn.FireEvery == copy.FireEvery, "Full plan reproduces from seed");
                }
                if (plan.Kind == EncounterKind.DepthFormation)
                {
                    var s = plan.Spawns;
                    Check(s[0].Y == s[1].Y && s[2].Y == s[3].Y && s[0].X < s[1].X && s[2].X < s[3].X,
                        "Piercing targets align in depth, in two distinct rows");
                    Check(s[2].SpawnAt > s[1].SpawnAt + 1f, "Depth formation arrives in separate groups");
                }
                if (plan.Kind == EncounterKind.DenseCluster)
                    Check(EncounterChapter.ClusterSpreadAt - lastSpawn - EncounterChapter.EntryDuration >= 2f,
                        "All enemies share at least two seconds of clustered opportunity");
                if (plan.Kind == EncounterKind.StaggeredCrossfire)
                    for (int i = 1; i < plan.Spawns.Length; i++)
                        Check((plan.Spawns[i].Y > 0.5f) != (plan.Spawns[i - 1].Y > 0.5f), "Attack schedule alternates upper and lower lines");
            }
        }
        bool rejected = false;
        try { EncounterChapter.CreateWave(7, 0, 3); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Boss wave cannot accidentally generate ordinary encounters");
        rejected = false;
        try { EncounterChapter.CreateWave(1, 0, 0); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Missing prefabs cannot silently produce invalid enemy indices");
        Check(EncounterChapter.CreateWave(1, 0, 3)[0].Spawns[0].EnemyIndex !=
            EncounterChapter.CreateWave(1, 1, 3)[0].Spawns[0].EnemyIndex, "Different seeds can change hull composition");
        Console.WriteLine("PASS: fixed chapter, seed reproduction, entry warnings, cluster window, crossfire, safety bounds, density limits");
    }
}
