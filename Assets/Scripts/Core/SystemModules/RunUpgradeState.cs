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
            new RunUpgradeDefinition(RunUpgradeId.Weapons, "大口径弹药", "每级武器伤害 +20%。\n对导弹直击和爆炸伤害同样生效。", 5),
            new RunUpgradeDefinition(RunUpgradeId.RapidFire, "快速供弹", "每级主炮射速 +15%。\n对极速模式下的射速同样生效。", 5),
            new RunUpgradeDefinition(RunUpgradeId.Hull, "强化机体", "每级增加基础机体上限的 20%。\n立即恢复等量生命值。", 5),
            new RunUpgradeDefinition(RunUpgradeId.Dodge, "节能翻滚", "每级减少基础翻滚能耗的 15%。\n仍受极速模式的能耗倍率影响。", 4),
            new RunUpgradeDefinition(RunUpgradeId.Thrusters, "矢量推进器", "每级移动速度 +10%。\n可与极速模式的速度加成叠加。", 4),
            new RunUpgradeDefinition(RunUpgradeId.Capacitor, "战斗电容", "每级使命中或击杀额外恢复 1 点能量。\n极速模式期间无法恢复能量。", 4),
            new RunUpgradeDefinition(RunUpgradeId.Pierce, "穿透弹", "主炮子弹可穿透一个敌人。\n每发最多命中两个不同敌人。\n极速模式下同样生效。", 1),
            new RunUpgradeDefinition(RunUpgradeId.Split, "分裂弹", "首次穿透命中时发射两枚侧向子弹。\n每枚造成原弹伤害的 40%。\n主炮伤害 -15%，侧向子弹无法再次分裂。", 1, RunUpgradeId.Pierce),
            new RunUpgradeDefinition(RunUpgradeId.BlastRadius, "扩域弹头", "导弹爆炸半径 +50%。\n发射冷却时间 +25%。\n每次改装额外补给 2 枚导弹。", 1),
            new RunUpgradeDefinition(RunUpgradeId.ClusterWarhead, "连锁引爆", "导弹击杀最多触发 4 次二次爆炸。\n每次造成 60% 爆炸伤害，半径为原来的 75%。\n二次爆炸无法再次触发连锁。", 1, RunUpgradeId.BlastRadius),
            new RunUpgradeDefinition(RunUpgradeId.Reclaimer, "弹头回收器", "导弹每击杀 3 个敌人返还 1 枚导弹。\n二次爆炸的击杀同样计入。\n每次引爆最多返还 1 枚导弹。", 1, RunUpgradeId.ClusterWarhead),
            new RunUpgradeDefinition(RunUpgradeId.Slipstream, "翻滚加速器", "翻滚后射速 +50%，持续 2.5 秒。\n翻滚能耗 +20%。\n再次翻滚会刷新持续时间。", 1),
            new RunUpgradeDefinition(RunUpgradeId.KineticBattery, "动能电池", "翻滚及极速模式的能量消耗转化为充能，最多 100 点。\n下轮主炮齐射消耗全部充能，伤害最高 +200%。\n停止射击可积攒更强的齐射。", 1, RunUpgradeId.Slipstream),
            new RunUpgradeDefinition(RunUpgradeId.OverdriveRelay, "极速继电器", "进入极速模式时获得 50 点充能。\n退出极速模式时获得翻滚射速加成。\n极速模式期间仍无法恢复能量。", 1, RunUpgradeId.KineticBattery),
            // Repeatable supplies keep three distinct choices available in long endless runs.
            new RunUpgradeDefinition(RunUpgradeId.Repair, "战地维修", "立即恢复机体上限 50% 的生命值。\n即时补给，不增加机体上限。", 0),
            new RunUpgradeDefinition(RunUpgradeId.Missiles, "导弹补给", "立即获得 5 枚导弹。\n即时补给。", 0),
            new RunUpgradeDefinition(RunUpgradeId.Energy, "能量电池", "完成改装后恢复 50 点能量。\n即时补给，并结束当前极速模式。", 0)
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
