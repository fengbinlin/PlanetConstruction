using UnityEngine;
using System.Collections;
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

    // ★ 新增：Debug模式
    public bool debugMode = false;
    public float debugSpawnInterval = 10f; // Debug模式下额外生成的间隔

    private float spawnTimer;
    private float debugSpawnTimer; // Debug模式专用计时器

    void Awake()
    {
        Instance = this;
        currentEnemyHP = baseEnemyHP;
    }

    void Update()
    {
        // 原有生成逻辑保持不变
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

        // ★ 新增：Debug模式下的额外生成
        if (debugMode)
        {
            debugSpawnTimer += Time.deltaTime;
            if (debugSpawnTimer >= debugSpawnInterval)
            {
                DebugSpawnEnemy();
                debugSpawnTimer = 0f;
            }
        }
    }

    // ★ 新增：Debug模式专用的生成方法
    void DebugSpawnEnemy()
    {
        Debug.Log($"Debug模式生成敌人 - 当前敌人数量: {enemies.Count}, 敌人HP: {currentEnemyHP}");
        SpawnEnemy(true); // 传递true表示这是Debug模式生成的
    }

    void SpawnEnemy(bool isDebugSpawn = false)
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
            // ★ 修改：Debug模式生成的敌人会有特殊标记
            if (isDebugSpawn)
            {
                enemy.SetMaxHp(currentEnemyHP * 1.2f); // Debug模式的敌人血量增加20%
                enemyObj.name = "DebugEnemy"; // 添加标记便于识别
            }
            else
            {
                enemy.SetMaxHp(currentEnemyHP);
            }
            
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
        }
    }

    // ★ 新增：在Inspector中切换Debug模式的便捷方法
    public void ToggleDebugMode()
    {
        debugMode = !debugMode;
        debugSpawnTimer = 0f;
        Debug.Log($"Debug模式: {(debugMode ? "开启" : "关闭")}");
    }
}