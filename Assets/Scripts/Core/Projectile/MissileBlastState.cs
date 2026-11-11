using System.Collections.Generic;
using Core.SystemModules;

// One detonation, many bounded areas. Shared with the standalone regression checks.
public sealed class MissileBlastState
{
    private readonly HashSet<int> damagedInArea = new HashSet<int>();
    private bool detonated;
    public int Kills { get; private set; }
    public int SecondarySeeds { get; private set; }

    public void Reset()
    {
        detonated = false;
        Kills = SecondarySeeds = 0;
        damagedInArea.Clear();
    }

    public bool TryDetonate()
    {
        if (detonated) return false;
        detonated = true;
        return true;
    }

    public void BeginArea() => damagedInArea.Clear();
    public bool TryDamage(int target) => damagedInArea.Add(target);
    public bool RecordKill(bool canSeedSecondary)
    {
        Kills++;
        if (!canSeedSecondary || SecondarySeeds >= CombatBuildState.MaxSecondaryBlasts) return false;
        SecondarySeeds++;
        return true;
    }
}
