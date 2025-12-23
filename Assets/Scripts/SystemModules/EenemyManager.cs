using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    WaitForSeconds _waitTimeBetweenSpawn;
    WaitForSeconds _waitTimeBetweenWave;
    WaitUntil _waitUntilAllEnemyDead;

    protected override void Awake()
    {
        base.Awake();
        _enemyList = new List<GameObject>();
        _waitTimeBetweenSpawn = new WaitForSeconds(timeBetweenSpawn);
        _waitTimeBetweenWave = new WaitForSeconds(timeBetweenWave);
        _waitUntilAllEnemyDead = new WaitUntil(() => _enemyList.Count == 0);
        // WaitUntil 参数为一个bool类型的委托 当委托返回true时 WaitUntil才会结束
    }

    IEnumerator Start()
    {
        while (spawnEnemy && GameManager.GameState != GameState.GameOver)
        {
            waveUI.SetActive(true);
            yield return _waitTimeBetweenWave;
            waveUI.SetActive(false);
            yield return StartCoroutine(nameof(RandomlySpawnEnemyCoroutine));
        }
    }

    IEnumerator RandomlySpawnEnemyCoroutine()
    {
        if (_waveNumber % bossWaveNumber == 0)
        {
            _enemyList.Add(PoolManger.Release(boss));
        }
        else
        {
            _enemyAmount = Mathf.Clamp(_enemyAmount, minEnemyAmount + _waveNumber / bossWaveNumber, maxEnemyAmount);
            for (var i = 0; i < _enemyAmount; i++)
            {
                _enemyList.Add(PoolManger.Release(enemies[Random.Range(0, enemies.Length)]));
                yield return _waitTimeBetweenSpawn;
            }
        }
        yield return _waitUntilAllEnemyDead;
        
        _waveNumber++;
    }
    
    public void RemoveEnemyFromList(GameObject enemy) => _enemyList.Remove(enemy);
}