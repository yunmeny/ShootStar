namespace Core.SystemModules
{
    public static class BuildTestChapter
    {
        public const int RewardWaves = 6;
        public const int BossWave = RewardWaves + 1;
        public static bool IsBoss(int wave) => wave == BossWave;
        public static bool IsComplete(int nextWave) => nextWave > BossWave;
        public static int EnemyCount(int wave) => 4 + ((wave - 1) / 2) * 2;
    }
}
