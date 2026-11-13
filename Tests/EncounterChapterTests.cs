using System;
using Core.SystemModules;

// Standalone: compile with EncounterChapter.cs, EncounterMotion.cs and BuildTestChapter.cs.
public static class EncounterChapterTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Main()
    {
        var expected = new[] { EncounterKind.LaneStream, EncounterKind.VerticalColumn,
            EncounterKind.ParallelLanes, EncounterKind.MixedSpeedPass };
        for (int i = 0; i < expected.Length; i++)
        {
            var wave = EncounterChapter.CreateWave(i + 1, EncounterChapter.DefaultSeed, 3);
            Check(wave.Length == 1 && wave[0].Kind == expected[i], "First four waves introduce each observed formation");
        }
        var fifth = EncounterChapter.CreateWave(5, 3103, 3);
        Check(fifth.Length == 2 && fifth[0].Kind == expected[0] && fifth[1].Kind == expected[1], "Wave five combines train and column");
        var sixth = EncounterChapter.CreateWave(6, 3103, 3);
        Check(sixth.Length == 4, "Sixth reward wave covers all four formations");
        foreach (var kind in expected) Check(Array.Exists(sixth, plan => plan.Kind == kind), "All formations are covered");

        foreach (int seed in new[] { int.MinValue, -1, 0, 1, 3103, int.MaxValue })
        for (int types = 1; types <= 3; types++)
        for (int wave = 1; wave <= 6; wave++)
        {
            var plans = EncounterChapter.CreateWave(wave, seed, types);
            EncounterChapter.CreateWave(6, seed == int.MaxValue ? 0 : seed + 1, types);
            var copies = EncounterChapter.CreateWave(wave, seed, types);
            for (int phase = 0; phase < plans.Length; phase++)
            {
                var plan = plans[phase];
                Check(plan.Spawns.Length <= EncounterChapter.MaxActiveEnemies, "No-kill run stays within enemy budget");
                float lastSpawn = -1f, phaseEnd = 0f;
                for (int i = 0; i < plan.Spawns.Length; i++)
                {
                    var spawn = plan.Spawns[i];
                    var copy = copies[phase].Spawns[i];
                    Check(spawn.EnemyIndex >= 0 && spawn.EnemyIndex < types, "All configured prefab counts are supported");
                    Check(spawn.SpawnAt >= lastSpawn, "Spawns remain sorted, including simultaneous column entries");
                    lastSpawn = spawn.SpawnAt;
                    Check(spawn.FireAt >= spawn.EntryAt && spawn.FireAt < spawn.ExitAt, "First volley is scheduled on screen");
                    Check(spawn.FireEvery >= plan.Spawns.Length * EncounterChapter.MinVolleyGap, "Volley schedule respects shared budget");
                    Check(spawn.CompleteAt > spawn.ExitAt && spawn.CompleteAt < 30f, "Every slot finishes even without kills");
                    Check(spawn.Kind == copy.Kind && spawn.EnemyIndex == copy.EnemyIndex && spawn.SpawnAt == copy.SpawnAt &&
                        spawn.Y == copy.Y && spawn.Speed == copy.Speed && spawn.FireAt == copy.FireAt && spawn.FireEvery == copy.FireEvery,
                        "Seed and call order reproduce the complete plan");
                    CheckMotion(spawn);
                    phaseEnd = Math.Max(phaseEnd, spawn.CompleteAt);
                }
                foreach (var spawn in plan.Spawns)
                    Check(EncounterMotion.Sample(spawn, phaseEnd + 0.01f).Stage == EncounterMotionStage.Complete,
                        "A phase can finish with every ship escaping");
            }
        }
        CheckFormations();
        bool rejected = false;
        try { EncounterChapter.CreateWave(7, 0, 3); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Boss wave never generates an ordinary encounter");
        rejected = false;
        try { EncounterChapter.CreateWave(1, 0, 0); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Missing prefabs cannot silently generate invalid indices");
        Console.WriteLine("PASS: six reward waves, seed isolation, full-screen constant-speed passes, formations, fire windows and bounded escape");
    }

    private static void CheckMotion(EncounterSpawn spawn)
    {
        var entry = EncounterMotion.Sample(spawn, spawn.SpawnAt);
        Check(entry.X > 1f && !entry.CanFire && entry.Stage == EncounterMotionStage.Entry, "Entry starts off the right edge");
        Check(!EncounterMotion.Sample(spawn, spawn.ExitAt).CanFire, "Off-screen retreat cannot fire");
        var end = EncounterMotion.Sample(spawn, spawn.CompleteAt + 0.01f);
        Check(end.X < 0f && end.Stage == EncounterMotionStage.Complete && !end.CanFire, "Flight ends beyond the LEFT edge");
        bool visitedRight = false, visitedLeft = false;
        var previous = entry;
        for (float t = spawn.SpawnAt + 0.025f; t <= spawn.CompleteAt; t += 0.025f)
        {
            var current = EncounterMotion.Sample(spawn, t);
            Check(current.X < previous.X && current.Y == previous.Y, "Observed passes never stop, turn back, oscillate or swap lanes");
            Check(Math.Abs(previous.X - current.X - spawn.Speed * 0.025f) < 0.00001f,
                "Speed stays constant across entry, midline and exit boundaries");
            if (current.Stage == EncounterMotionStage.Combat)
            {
                Check(current.X >= 0f && current.X <= 1f && current.CanFire, "Combat stage covers the full visible flight");
                visitedRight |= current.X > 0.5f;
                visitedLeft |= current.X < 0.5f;
            }
            previous = current;
        }
        Check(visitedRight && visitedLeft, "Every ship traverses BOTH halves of the screen");
        float pausedClock = (spawn.EntryAt + spawn.ExitAt) * 0.5f;
        var a = EncounterMotion.Sample(spawn, pausedClock);
        EncounterMotion.Sample(spawn, spawn.CompleteAt + 10f);
        var b = EncounterMotion.Sample(spawn, pausedClock);
        Check(a.X == b.X && a.Y == b.Y && a.Stage == b.Stage, "Pause and pooled reuse cannot retain hidden movement progress");
    }

    private static void CheckFormations()
    {
        var train = EncounterChapter.CreateWave(1, 3103, 3)[0].Spawns;
        for (int i = 1; i < train.Length; i++)
        {
            var front = EncounterMotion.Sample(train[i - 1], 5f);
            var rear = EncounterMotion.Sample(train[i], 5f);
            Check(front.Y == rear.Y && rear.X > front.X, "Train preserves depth alignment while flying");
        }
        var column = EncounterChapter.CreateWave(2, 3103, 3)[0].Spawns;
        var top = EncounterMotion.Sample(column[0], 4f);
        for (int i = 1; i < column.Length; i++)
        {
            var ship = EncounterMotion.Sample(column[i], 4f);
            Check(ship.X == top.X && ship.Y < column[i - 1].Y,
                "Four-ship column enters simultaneously and preserves vertical alignment");
        }
        var lanes = EncounterChapter.CreateWave(3, 3103, 3)[0].Spawns;
        Check(lanes[0].SpawnAt == lanes[1].SpawnAt && lanes[0].Y > 0.5f && lanes[1].Y < 0.5f,
            "Parallel lanes enter together and stay on opposite sides vertically");
        var mixed = EncounterChapter.CreateWave(4, 3103, 3)[0].Spawns;
        Check(mixed[0].Speed < mixed[1].Speed && mixed[1].EntryAt < mixed[0].ExitAt,
            "Slow pass overlaps the later fast column instead of inventing an escort orbit");
        // Independent visual measurement: the upper ship moves roughly 0.20 widths in 1.25 seconds.
        var a = EncounterMotion.Sample(train[0], 4f);
        var b = EncounterMotion.Sample(train[0], 5.25f);
        Check(Math.Abs((a.X - b.X) - 0.205f) < 0.01f, "Fast flight matches the measured video displacement");
        a = EncounterMotion.Sample(mixed[0], 10f);
        b = EncounterMotion.Sample(mixed[0], 15f);
        Check(Math.Abs((a.X - b.X) - 0.26f) < 0.02f, "Slow flight matches the measured five-second displacement");
    }
}
