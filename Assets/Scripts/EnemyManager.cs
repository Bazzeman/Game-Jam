using Bas.Pennings.DevTools;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class EnemyManager : AbstractSingleton<EnemyManager>
{
    [SerializeField] private float _enemySpawningDelay = 1;

    [Header("References")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform _enemyObjectsParent;
    [SerializeField] private SpawnPoint[] _enemySpawnPoints;
    private GameObject[] enemyObjects;
    private Coroutine enemySpawningRoutine;

    private void Start() => StartEnemySpawning();

    private void OnValidate() => _enemyObjectsParent = _enemyObjectsParent != null ? _enemyObjectsParent : transform;

    public void ResetEnemies()
    {
        StopEnemySpawning();
        DestroyEnemies();
        StartEnemySpawning();
    }

    private void DestroyEnemies()
    {
        foreach (var enemy in enemyObjects) Destroy(enemy);
        enemyObjects = null;
    }

    private void StopEnemySpawning()
    {
        if (enemySpawningRoutine != null)
        {
            StopCoroutine(enemySpawningRoutine);
            enemySpawningRoutine = null;
        }
    }

    private void StartEnemySpawning()
    {
        enemySpawningRoutine = StartCoroutine(InstantiateEnemiesWithDelay());

        IEnumerator InstantiateEnemiesWithDelay()
        {
            var totalEnemies = _enemySpawnPoints.Sum((sp) => sp.EnemyCount);
            enemyObjects = new GameObject[totalEnemies];

            for (int i = 0, index = 0; i < _enemySpawnPoints.Length; i++)
            {
                var spawnPoint = _enemySpawnPoints[i];
                for (int j = 0; j < spawnPoint.EnemyCount; j++, index++)
                {
                    enemyObjects[index] = InstantiateEnemy(spawnPoint.SpawnPosition);
                    yield return new WaitForSeconds(_enemySpawningDelay);
                }
            }
        }
    }

    private GameObject InstantiateEnemy(Transform transform)
        => Instantiate(
            _enemyPrefab,
            transform.position,
            transform.rotation,
            _enemyObjectsParent.transform);

    [Serializable]
    private struct SpawnPoint
    {
        [Description("Totally optional, will only be used in the Unity editor UI")]
        public string Name;

        [Description("The transform at which enemies will be spawned")]
        public Transform SpawnPosition;

        [Description("The amount of enemies which will spawn at the spawn position")]
        public int EnemyCount;
    }
}
