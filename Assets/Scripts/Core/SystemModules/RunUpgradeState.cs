using System;
using System.Collections.Generic;

namespace Core.SystemModules
{
    public enum RunUpgradeId { Weapons, RapidFire, Hull, Dodge, Thrusters, Capacitor, Repair, Missiles, Energy, Pierce, Split,
        BlastRadius, ClusterWarhead, Reclaimer, Slipstream, KineticBattery, OverdriveRelay }
    public enum BuildFocus { None, MainGun, Missiles, Maneuver }

    public sealed class RunUpgradeDefinition
    {
        public RunUpgradeId Id { get; }
        public string Title { get; }
        public string Description { get; }
        public int MaxRank { get; }
        public bool IsService => MaxRank == 0;
        public RunUpgradeId? Prerequisite { get; }

        public RunUpgradeDefinition(RunUpgradeId id, string title, string description, int maxRank, RunUpgradeId? prerequisite = null)
        {
            Id = id;
            Title = title;
            Description = description;
            MaxRank = maxRank;
            Prerequisite = prerequisite;
        }
    }

    // Scene-owned data: never written to the player prefab or the persistent save.
    public sealed class RunUpgradeState
    {
        public static readonly IReadOnlyList<RunUpgradeDefinition> Catalog = Array.AsReadOnly(new[]
        {
            new RunUpgradeDefinition(RunUpgradeId.Weapons, "HEAVY CALIBER", "+20% weapon damage per rank.\nIncludes missile impact and explosion damage.", 5),
            new RunUpgradeDefinition(RunUpgradeId.RapidFire, "RAPID CHAMBER", "+15% main gun fire rate per rank.\nAlso improves Overdrive fire rate.", 5),
            new RunUpgradeDefinition(RunUpgradeId.Hull, "REINFORCED HULL", "+20% base maximum hull per rank.\nRepair the same amount immediately.", 5),
            new RunUpgradeDefinition(RunUpgradeId.Dodge, "EFFICIENT ROLL", "Rolling costs 15% less base energy per rank.\nOverdrive's cost modifier still applies.", 4),
            new RunUpgradeDefinition(RunUpgradeId.Thrusters, "VECTOR THRUSTERS", "+10% movement speed per rank.\nStacks with Overdrive's speed bonus.", 4),
            new RunUpgradeDefinition(RunUpgradeId.Capacitor, "COMBAT CAPACITOR", "+1 energy per hit or kill per rank.\nEnergy cannot recharge during Overdrive.", 4),
            new RunUpgradeDefinition(RunUpgradeId.Pierce, "PIERCING ROUNDS", "Main gun shots pass through one enemy.\nEach shot can hit up to two different enemies.\nAlso works during Overdrive.", 1),
            new RunUpgradeDefinition(RunUpgradeId.Split, "FORKED ROUNDS", "First piercing hit releases two side shots.\nEach deals 40% of the parent shot's damage.\nMain gun damage -15%; side shots cannot fork.", 1, RunUpgradeId.Pierce),
            new RunUpgradeDefinition(RunUpgradeId.BlastRadius, "WIDE WARHEAD", "Missile blast radius +50%.\nLauncher cooldown +25%.\nEvery refit supplies 2 missiles.", 1),
            new RunUpgradeDefinition(RunUpgradeId.ClusterWarhead, "CHAIN DETONATION", "Missile kills cause up to 4 secondary blasts.\nEach deals 60% blast damage at 75% radius.\nSecondary blasts cannot chain again.", 1, RunUpgradeId.BlastRadius),
            new RunUpgradeDefinition(RunUpgradeId.Reclaimer, "WARHEAD RECLAIMER", "Every 3 missile kills return 1 missile.\nIncludes secondary blast kills.\nAt most 1 missile returned per detonation.", 1, RunUpgradeId.ClusterWarhead),
            new RunUpgradeDefinition(RunUpgradeId.Slipstream, "ROLL ACCELERATOR", "After a roll: +50% fire rate for 2.5 seconds.\nRolling costs 20% more energy.\nFurther rolls refresh the duration.", 1),
            new RunUpgradeDefinition(RunUpgradeId.KineticBattery, "KINETIC BATTERY", "Roll and Overdrive energy builds charge (100 max).\nNext main gun volley consumes it: up to +200% damage.\nStop firing to save a stronger volley.", 1, RunUpgradeId.Slipstream),
            new RunUpgradeDefinition(RunUpgradeId.OverdriveRelay, "OVERDRIVE RELAY", "Entering Overdrive adds 50 charge.\nLeaving Overdrive grants the roll fire-rate boost.\nOverdrive still cannot recharge energy.", 1, RunUpgradeId.KineticBattery),
            // Repeatable supplies keep three distinct choices available in long endless runs.
            new RunUpgradeDefinition(RunUpgradeId.Repair, "FIELD REPAIR", "Restore 50% of maximum hull.\nOne-time service; does not increase maximum hull.", 0),
            new RunUpgradeDefinition(RunUpgradeId.Missiles, "MISSILE CACHE", "Receive 5 missiles.\nOne-time service.", 0),
            new RunUpgradeDefinition(RunUpgradeId.Energy, "ENERGY CELL", "Restore 50 energy after leaving refit.\nOne-time service; ends the current Overdrive.", 0)
        });

        private readonly Dictionary<RunUpgradeId, int> ranks = new Dictionary<RunUpgradeId, int>();
        public int Selections { get; private set; }
        public int Rank(RunUpgradeId id) => ranks.TryGetValue(id, out int rank) ? rank : 0;
        public float DamageMultiplier => 1f + 0.2f * Rank(RunUpgradeId.Weapons);
        public float FireRateMultiplier => 1f + 0.15f * Rank(RunUpgradeId.RapidFire);
        public float HullMultiplier => 1f + 0.2f * Rank(RunUpgradeId.Hull);
        public float DodgeCostMultiplier => 1f - 0.15f * Rank(RunUpgradeId.Dodge);
        public float MoveSpeedMultiplier => 1f + 0.1f * Rank(RunUpgradeId.Thrusters);
        public int EnergyBonus => Rank(RunUpgradeId.Capacitor);
        public bool HasPierce => Rank(RunUpgradeId.Pierce) > 0;
        public bool HasSplit => Rank(RunUpgradeId.Split) > 0;
        public float MainGunDamageMultiplier => DamageMultiplier * (HasSplit ? 0.85f : 1f);
        public bool HasWideWarhead => Rank(RunUpgradeId.BlastRadius) > 0;
        public bool HasChainDetonation => Rank(RunUpgradeId.ClusterWarhead) > 0;
        public bool HasReclaimer => Rank(RunUpgradeId.Reclaimer) > 0;
        public bool HasSlipstream => Rank(RunUpgradeId.Slipstream) > 0;
        public bool HasKineticBattery => Rank(RunUpgradeId.KineticBattery) > 0;
        public bool HasOverdriveRelay => Rank(RunUpgradeId.OverdriveRelay) > 0;
        public float MissileRadiusMultiplier => HasWideWarhead ? 1.5f : 1f;
        public float MissileCooldownMultiplier => HasWideWarhead ? 1.25f : 1f;
        public float ManeuverDodgeCostMultiplier => HasSlipstream ? 1.2f : 1f;
        public int DodgeEnergyCost(int baseCost, float overdriveFactor) => Math.Max(1,
            (int)Math.Ceiling(baseCost * DodgeCostMultiplier * ManeuverDodgeCostMultiplier * overdriveFactor - 0.0001f));

        public static RunUpgradeDefinition Definition(RunUpgradeId id)
        {
            foreach (var definition in Catalog) if (definition.Id == id) return definition;
            return null;
        }

        public bool CanApply(RunUpgradeDefinition definition) =>
            (!definition.Prerequisite.HasValue || Rank(definition.Prerequisite.Value) > 0) &&
            (definition.IsService || Rank(definition.Id) < definition.MaxRank);

        public RunUpgradeDefinition[] RollChoices(Random random, bool focusMainGun = false)
            => RollChoices(random, focusMainGun ? BuildFocus.MainGun : BuildFocus.None);

        public RunUpgradeDefinition[] RollChoices(Random random, BuildFocus focus)
        {
            var pool = new List<RunUpgradeDefinition>();
            foreach (var definition in Catalog)
                if (!definition.IsService && CanApply(definition))
                    pool.Add(definition);
            // Supplies only enter after permanent options start running out.
            if (pool.Count < 3)
                foreach (var definition in Catalog)
                    if (definition.IsService) pool.Add(definition);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var swap = pool[i];
                pool[i] = pool[j];
                pool[j] = swap;
            }
            var choices = pool.GetRange(0, 3).ToArray();
            // The test chapter offers the next missing link every wave until the player takes it.
            // Other modes retain the ordinary random pool and the same prerequisites.
            RunUpgradeId? desired = NextFocusedUpgrade(focus);
            if (desired.HasValue)
            {
                var definition = pool.Find(item => item.Id == desired.Value);
                if (definition != null && Array.FindIndex(choices, item => item.Id == desired.Value) < 0)
                    choices[0] = definition;
            }
            return choices;
        }

        public RunUpgradeId? NextFocusedUpgrade(BuildFocus focus)
        {
            switch (focus)
            {
                case BuildFocus.MainGun: return !HasPierce ? RunUpgradeId.Pierce : !HasSplit ? RunUpgradeId.Split : (RunUpgradeId?)null;
                case BuildFocus.Missiles: return !HasWideWarhead ? RunUpgradeId.BlastRadius : !HasChainDetonation ? RunUpgradeId.ClusterWarhead : !HasReclaimer ? RunUpgradeId.Reclaimer : (RunUpgradeId?)null;
                case BuildFocus.Maneuver: return !HasSlipstream ? RunUpgradeId.Slipstream : !HasKineticBattery ? RunUpgradeId.KineticBattery : !HasOverdriveRelay ? RunUpgradeId.OverdriveRelay : (RunUpgradeId?)null;
                default: return null;
            }
        }

        public bool TryApply(RunUpgradeId id)
        {
            foreach (var definition in Catalog)
            {
                if (definition.Id != id) continue;
                if (!CanApply(definition)) return false;
                if (!definition.IsService) ranks[id] = Rank(id) + 1;
                Selections++;
                return true;
            }
            return false;
        }
    }
}
