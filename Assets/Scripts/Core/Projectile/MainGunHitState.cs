// Per-shot hit budget. A maximum of two targets avoids per-projectile collections/allocations.
public struct MainGunHitState
{
    private bool pierce;
    private bool split;
    private int firstTarget;
    private int secondTarget;
    private int blockedTarget;
    private bool spent;

    public void Reset(bool piercing, bool splitting, bool fragment, int parentTarget = 0)
    {
        pierce = piercing && !fragment;
        split = splitting && pierce;
        firstTarget = secondTarget = 0;
        blockedTarget = parentTarget;
        spent = false;
    }

    public bool TryHit(int target, out bool keepAlive, out bool fork)
    {
        keepAlive = fork = false;
        if (spent || target == blockedTarget || target == firstTarget || target == secondTarget) return false;
        if (firstTarget == 0)
        {
            firstTarget = target;
            keepAlive = pierce;
            fork = split;
        }
        else secondTarget = target;
        spent = !keepAlive;
        return true;
    }
}
