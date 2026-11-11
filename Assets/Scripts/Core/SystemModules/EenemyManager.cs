using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SystemModules
{
    public class EenemyManager : Singleton<EenemyManager>
    {
        public GameObject GetRandomEnemy => _enemyList.Count == 0 ? null : _enemyList[Random.Range(0, _enemyList.Count)];

        public int WaveNumber => _waveNumber;
        public float TimeBetweenWave => timeBetweenWave;

        [SerializeField] bool spawnEnemy = true;
        [SerializeField] GameObject waveUI;
        [SerializeField] GameObject[] enemies;
        [SerializeField] private GameObject boss;
        [SerializeField] float timeBetweenSpawn = 1f;
        [SerializeField] float timeBetweenWave = 3f;
        [SerializeField] int maxEnemyAmount = 10;
        [SerializeField] int minEnemyAmount = 3;
        [SerializeField] int bossWaveNumber = 3;

        List<GameObject> _enemyList;
        int _waveNumber = 1;
        int _enemyAmount = 0;
        RunUpgradeController _upgrades;

        WaitForSeconds _waitTimeBetweenSpawn;
        WaitForSeconds _waitTimeBetweenWave;
        WaitUntil _waitUntilAllEnemyDead;

        protected override void Awake()
        {
            base.Awake();
            _enemyList = new List<GameObject>();
            _upgrades = GetComponent<RunUpgradeController>();
            if (!_upgrades) _upgrades = gameObject.AddComponent<RunUpgradeController>();
            _waitTimeBetweenSpawn = new WaitForSeconds(timeBetweenSpawn);
            _waitTimeBetweenWave = new WaitForSeconds(timeBetweenWave);
            _waitUntilAllEnemyDead = new WaitUntil(() => _enemyList.Count == 0);
            // WaitUntil 参数为一个bool类型的委托 当委托返回true时 WaitUntil才会结束
        }

        void OnEnable() => GameManager.onGameOver += StopSpawning;
        void OnDisable() => GameManager.onGameOver -= StopSpawning;

        void StopSpawning()
        {
            StopAllCoroutines();
            if (waveUI) waveUI.SetActive(false);
        }

        IEnumerator Start()
        {
            while (spawnEnemy && GameManager.GameState != GameState.GameOver)
            {
                waveUI.SetActive(true);
                yield return _waitTimeBetweenWave;
                waveUI.SetActive(false);
                if (GameManager.GameState == GameState.GameOver) yield break;
                yield return StartCoroutine(nameof(RandomlySpawnEnemyCoroutine));
                if ((GameManager.Instance.Mode == GameMode.Story && _waveNumber > Mathf.Max(1, bossWaveNumber)) ||
                    (GameManager.Instance.IsBuildTest && BuildTestChapter.IsComplete(_waveNumber)))
                {
                    GameManager.Instance.EndRun(true);
                    yield break;
                }
                if (GameManager.GameState == GameState.Play)
                    yield return _upgrades.ChooseAfterWave(_waveNumber - 1);
            }
        }

        IEnumerator RandomlySpawnEnemyCoroutine()
        {
            bool buildTest = GameManager.Instance.IsBuildTest;
            if (buildTest ? BuildTestChapter.IsBoss(_waveNumber) : _waveNumber % Mathf.Max(1, bossWaveNumber) == 0)
            {
                _enemyList.Add(PoolManger.Release(boss));
            }
            else
            {
                _enemyAmount = buildTest ? BuildTestChapter.EnemyCount(_waveNumber) :
                    Mathf.Clamp(_enemyAmount, minEnemyAmount + _waveNumber / Mathf.Max(1, bossWaveNumber), maxEnemyAmount);
                for (var i = 0; i < _enemyAmount; i++)
                {
                    var prefab = enemies[buildTest ? (_waveNumber - 1 + i / 2) % enemies.Length : Random.Range(0, enemies.Length)];
                    var enemy = PoolManger.Release(prefab);
                    _enemyList.Add(enemy);
                    if (buildTest && enemy.TryGetComponent<EnemyController>(out var controller))
                    {
                        var view = Viewport.Instance;
                        int rows = _enemyAmount / 2;
                        float y = Mathf.Lerp(view.MinY, view.MaxY, 0.5f + ((i / 2) - (rows - 1) * 0.5f) * 0.2f);
                        var slot = new Vector3(Mathf.Lerp(view.MiddleX, view.MaxX, i % 2 == 0 ? 0.2f : 0.65f), y);
                        controller.SetBuildTestFormation(slot);
                    }
                    yield return _waitTimeBetweenSpawn;
                }
            }
            yield return _waitUntilAllEnemyDead;
        
            _waveNumber++;
        }
    
        public void RemoveEnemyFromList(GameObject enemy) => _enemyList.Remove(enemy);
    }
}
