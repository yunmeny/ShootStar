using System;
using System.Collections.Generic;
using Core.SystemModules;

// Standalone regression checks: compile with RunUpgradeState.cs; no Editor/test package required.
public static class RunUpgradeStateTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Main()
    {
        var run = new RunUpgradeState();
        var random = new Random(51);
        var seen = new HashSet<RunUpgradeId>();
        for (int draw = 0; draw < 1000; draw++)
        {
            var choices = run.RollChoices(random);
            Check(choices.Length == 3, "Always offer three choices");
            var unique = new HashSet<RunUpgradeId>();
            foreach (var choice in choices)
            {
                Check(unique.Add(choice.Id), "No duplicate cards in an offer");
                Check(!choice.IsService, "Permanent mods take priority in the initial pool");
                seen.Add(choice.Id);
            }
        }
        Check(seen.Count == 9 && !seen.Contains(RunUpgradeId.Split) && !seen.Contains(RunUpgradeId.ClusterWarhead) &&
            !seen.Contains(RunUpgradeId.KineticBattery), "Initial pool offers nine roots and excludes locked branch upgrades");
        int initialSelections = run.Selections;
        Check(!run.TryApply(RunUpgradeId.Split) && run.Selections == initialSelections, "Prerequisites are enforced when applying, too");
        Check(Array.Exists(run.RollChoices(random, true), item => item.Id == RunUpgradeId.Pierce), "Test chapter offers Pierce first");
        var focused = new RunUpgradeState();
        Check(focused.TryApply(RunUpgradeId.Pierce), "Pierce applies once");
        for (int i = 0; i < 100; i++)
        {
            var choices = focused.RollChoices(random, true);
            Check(Array.Exists(choices, item => item.Id == RunUpgradeId.Split), "Test chapter offers Split after Pierce");
            Check(new HashSet<RunUpgradeId>(Array.ConvertAll(choices, item => item.Id)).Count == 3,
                "Focused offers remain unique");
        }
        Check(focused.TryApply(RunUpgradeId.Split), "Split unlocks after Pierce");
        Check(Math.Abs(focused.MainGunDamageMultiplier - 0.85f) < 0.001f && focused.DamageMultiplier == 1f,
            "Split's damage tradeoff only affects the main gun");
        foreach (var definition in RunUpgradeState.Catalog)
        {
            if (definition.IsService) continue;
            for (int i = 0; i < definition.MaxRank; i++)
                Check(run.TryApply(definition.Id), "Apply eligible rank");
            Check(!run.TryApply(definition.Id), "Reject upgrades beyond their cap");
            for (int i = 0; i < 10; i++)
                foreach (var choice in run.RollChoices(random))
                    Check(choice.Id != definition.Id, "Capped mods leave the pool");
        }
        Check(Math.Abs(run.DamageMultiplier - 2f) < 0.001f, "Damage stacks additively against base");
        Check(Math.Abs(run.FireRateMultiplier - 1.75f) < 0.001f, "Fire rate stacks against base");
        Check(run.DodgeCostMultiplier > 0f, "Dodge cost remains positive");
        Check(!run.TryApply((RunUpgradeId)999), "Unknown upgrades do not mutate the run");
        int selections = run.Selections;
        for (int i = 0; i < 1000; i++)
        {
            var unique = new HashSet<RunUpgradeId>();
            foreach (var choice in run.RollChoices(random))
            {
                Check(choice.IsService && unique.Add(choice.Id), "Endless fallback is three distinct supplies");
                Check(run.TryApply(choice.Id), "Supplies stay repeatable");
            }
        }
        Check(run.Selections == selections + 3000, "Each accepted choice is counted once");
        var retry = new RunUpgradeState();
        Check(retry.Selections == 0 && retry.DamageMultiplier == 1f && retry.HullMultiplier == 1f &&
            retry.FireRateMultiplier == 1f && retry.EnergyBonus == 0, "New run has no inherited mods");
        Check(!retry.HasPierce && !retry.HasSplit && retry.MainGunDamageMultiplier == 1f, "Retry clears the main gun chain");
        int rewards = 0, bosses = 0;
        for (int wave = 1; !BuildTestChapter.IsComplete(wave); wave++)
        {
            if (BuildTestChapter.IsBoss(wave)) bosses++;
            else { rewards++; Check(BuildTestChapter.EnemyCount(wave) % 2 == 0, "Formation uses aligned pairs"); }
        }
        Check(rewards == 6 && bosses == 1, "Test chapter has exactly six reward waves and one final boss");
        TestHitBudget();
        TestRemainingBuilds(random);
        Console.WriteLine("PASS: rewards, all three build chains, missile blast limits, kinetic charge, boosts, refunds, reset");
    }

    private static void TestHitBudget()
    {
        var shot = new MainGunHitState();
        shot.Reset(true, true, false);
        Check(shot.TryHit(1, out bool alive, out bool split) && alive && split, "First piercing hit forks and survives");
        Check(!shot.TryHit(1, out _, out _), "Multiple colliders on one target cannot damage or fork twice");
        Check(shot.TryHit(2, out alive, out split) && !alive && !split, "Second enemy is hit without another fork");
        Check(!shot.TryHit(3, out _, out _), "Shot cannot hit a third enemy");
        shot.Reset(true, true, true, 1);
        Check(!shot.TryHit(1, out _, out _), "Side shot cannot re-hit its parent's first enemy");
        Check(shot.TryHit(2, out alive, out split) && !alive && !split, "Side shot cannot pierce or fork even with both mods");
        shot.Reset(false, false, false);
        Check(shot.TryHit(1, out alive, out split) && !alive && !split, "Pool reuse resets hit targets and budgets");
        shot.Reset(true, false, false);
        Check(shot.TryHit(1, out alive, out split) && alive && !split, "Pierce alone does not produce side shots");
    }

    private static void TestRemainingBuilds(Random random)
    {
        foreach (var focus in new[] { BuildFocus.Missiles, BuildFocus.Maneuver })
        {
            var run = new RunUpgradeState();
            for (int step = 0; step < 3; step++)
            {
                var next = run.NextFocusedUpgrade(focus);
                Check(next.HasValue, "Every remaining build has three links");
                for (int draw = 0; draw < 100; draw++)
                {
                    var choices = run.RollChoices(random, focus);
                    Check(Array.Exists(choices, item => item.Id == next.Value), "Focused chapter guarantees the next link");
                    Check(new HashSet<RunUpgradeId>(Array.ConvertAll(choices, item => item.Id)).Count == 3, "Focused options are distinct");
                    foreach (var choice in choices) Check(run.CanApply(choice), "Every offered option satisfies prerequisites");
                }
                Check(run.TryApply(next.Value), "Next build link applies");
            }
            Check(!run.NextFocusedUpgrade(focus).HasValue, "Completed branch stops forcing rewards");
            Check(run.DamageMultiplier == 1f && run.MainGunDamageMultiplier == 1f, "Other builds do not inherit Fork's damage tradeoff");
        }
        var missile = new RunUpgradeState();
        Check(!missile.TryApply(RunUpgradeId.ClusterWarhead) && !missile.TryApply(RunUpgradeId.Reclaimer), "Missile chain cannot skip prerequisites");
        missile.TryApply(RunUpgradeId.BlastRadius);
        Check(missile.MissileRadiusMultiplier == 1.5f && missile.MissileCooldownMultiplier == 1.25f, "Wide warhead has an explicit cooldown cost");
        var maneuver = new RunUpgradeState();
        Check(!maneuver.TryApply(RunUpgradeId.KineticBattery) && !maneuver.TryApply(RunUpgradeId.OverdriveRelay), "Maneuver chain cannot skip prerequisites");
        maneuver.TryApply(RunUpgradeId.Slipstream);
        Check(maneuver.ManeuverDodgeCostMultiplier == 1.2f, "Roll accelerator has an explicit energy cost");
        Check(maneuver.DodgeEnergyCost(25, 1f) == 30 && maneuver.DodgeEnergyCost(25, 2f) == 60,
            "Floating point drift cannot charge one extra unit for a roll");

        var combat = new CombatBuildState();
        combat.GrantBoost();
        combat.Tick(1f);
        Check(combat.FireRateMultiplier == 1.5f && combat.BoostRemaining == 1.5f, "Boost uses elapsed simulation time");
        combat.Tick(0f);
        Check(combat.BoostRemaining == 1.5f, "Paused simulation preserves boost duration");
        combat.GrantBoost();
        Check(combat.BoostRemaining == 2.5f, "Repeated rolls refresh rather than stack boost time");
        combat.Tick(3f);
        Check(combat.FireRateMultiplier == 1f, "Boost expires cleanly");
        combat.AddCharge(30);
        Check(Math.Abs(combat.ConsumeVolley() - 1.6f) < 0.001f && combat.ConsumeVolley() == 1f, "Charge affects one volley only");
        combat.AddCharge(1000);
        Check(combat.Charge == 100 && combat.ConsumeVolley() == 3f, "Burst charge and damage are bounded");
        Check(combat.RecordMissileKills(2) == 0 && combat.RecordMissileKills(1) == 1, "Reclaimer carries progress across missiles");
        Check(combat.RecordMissileKills(30) == 1, "At most one missile refunded per detonation");
        var retry = new CombatBuildState();
        Check(retry.Charge == 0 && retry.BoostRemaining == 0f && retry.ReclaimerProgress == 0, "Retry clears transient combat resources");

        var blast = new MissileBlastState();
        blast.Reset();
        Check(blast.TryDetonate() && !blast.TryDetonate(), "Missile detonates only once despite multiple collision callbacks");
        Check(blast.TryDamage(1) && !blast.TryDamage(1), "Multiple colliders cannot multiply blast damage");
        blast.BeginArea();
        Check(blast.TryDamage(1), "A separate secondary blast can damage a surviving target again");
        int seeds = 0;
        for (int i = 0; i < 20; i++) if (blast.RecordKill(true)) seeds++;
        Check(seeds == 4 && blast.SecondarySeeds == 4, "Secondary blast generation is capped at four");
        Check(!blast.RecordKill(false) && blast.Kills == 21, "Secondary kills count but cannot recursively seed explosions");
        blast.Reset();
        Check(blast.Kills == 0 && blast.SecondarySeeds == 0 && blast.TryDetonate() && blast.TryDamage(1), "Pooled missile resets its detonation and hit budgets");
    }
}
