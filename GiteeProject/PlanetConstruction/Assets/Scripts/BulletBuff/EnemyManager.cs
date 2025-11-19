using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    public List<Enemy> enemies = new List<Enemy>();

    public GameObject[] enemyPrefabs;
    public Vector2 spawnRangeY = new Vector2(-3f, 3f);
    public float spawnX = 8f;

    public float spawnInterval = 1f;      // 初始生成速度
    public float minSpawnInterval = 0.3f; // 最快生成速度
    public float spawnAccelerate = 0.98f; // 每次生成后乘的系数
    public int maxCount = 20;

    // ★ 新增：敌人生命值增长
    public float baseEnemyHP = 20f;  // 初始生命值
    public float currentEnemyHP;     // 当前生成敌人的生命值
    public float hpGrowthPerSpawn = 1f; // 每次生成增加多少血量

    private float spawnTimer;

    void Awake()
    {
        Instance = this;
        currentEnemyHP = baseEnemyHP;
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && enemies.Count < maxCount)
        {
            SpawnEnemy();
            spawnTimer = 0f;

            // 生成一次后加快速度
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval * spawnAccelerate);

            // ★ 每次生成敌人时，提升初始血量
            currentEnemyHP += hpGrowthPerSpawn;
        }
    }

    void SpawnEnemy()
    {
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3 pos = new Vector3(spawnX, Random.Range(spawnRangeY.x, spawnRangeY.y), 0);
        
        GameObject enemyObj = null;
        if (ObjectPoolManager.Instance != null)
        {
            enemyObj = ObjectPoolManager.Instance.SpawnEnemy(pos, Quaternion.identity);
        }
        else
        {
            enemyObj = Instantiate(prefab, pos, Quaternion.identity);
        }

        Scene battleScene = SceneManager.GetSceneByName("BattleScene");
        if (battleScene.IsValid())
        {
            try
            {
                // enemyObj.transform.SetParent(null);
                // SceneManager.MoveGameObjectToScene(enemyObj, battleScene);
            }
            catch
            {
                GameObject.Destroy(enemyObj);
            }
        }

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.SetMaxHp(currentEnemyHP);
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
        }
    }
}