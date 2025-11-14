using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    public List<Enemy> enemies = new List<Enemy>();

    public GameObject[] enemyPrefabs;
    public Vector2 spawnRangeY = new Vector2(-3f, 3f);
    public float spawnX = 8f;
    public float spawnInterval = 1f;
    public int maxCount = 20;

    private float spawnTimer;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && enemies.Count < maxCount)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3 pos = new Vector3(spawnX, Random.Range(spawnRangeY.x, spawnRangeY.y), 0);
        GameObject enemyObj = Instantiate(prefab, pos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        enemies.Add(enemy);
    }
}