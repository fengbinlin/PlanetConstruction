using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Header("对象池预设")]
    public GameObject bulletPrefab;
    public GameObject subBulletPrefab;
    public GameObject enemyPrefab;
    public GameObject bloodSplatterPrefab;

    [Header("对象池配置")]
    public int bulletPoolSize = 50;
    public int subBulletPoolSize = 100;
    public int enemyPoolSize = 30;
    public int bloodSplatterPoolSize = 20;

    public bool allowPoolExpansion = true;

    private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
    private Transform poolParent;

    private const string BULLET_POOL = "BulletPool";
    private const string SUB_BULLET_POOL = "SubBulletPool";
    private const string ENEMY_POOL = "EnemyPool";
    private const string BLOOD_POOL = "BloodPool";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        poolParent = new GameObject("PooledObjects").transform;
        poolParent.SetParent(transform);

        InitializePools();
    }

    void InitializePools()
    {
        if (bulletPrefab != null)
        {
            Transform bulletParent = new GameObject("Bullets").transform;
            bulletParent.SetParent(poolParent);
            pools[BULLET_POOL] = new ObjectPool(bulletPrefab, bulletPoolSize, allowPoolExpansion, bulletParent);
        }

        if (subBulletPrefab != null)
        {
            Transform subBulletParent = new GameObject("SubBullets").transform;
            subBulletParent.SetParent(poolParent);
            pools[SUB_BULLET_POOL] = new ObjectPool(subBulletPrefab, subBulletPoolSize, allowPoolExpansion, subBulletParent);
        }

        if (enemyPrefab != null)
        {
            Transform enemyParent = new GameObject("Enemies").transform;
            enemyParent.SetParent(poolParent);
            pools[ENEMY_POOL] = new ObjectPool(enemyPrefab, enemyPoolSize, allowPoolExpansion, enemyParent);
        }

        if (bloodSplatterPrefab != null)
        {
            Transform bloodParent = new GameObject("BloodSplatters").transform;
            bloodParent.SetParent(poolParent);
            pools[BLOOD_POOL] = new ObjectPool(bloodSplatterPrefab, bloodSplatterPoolSize, allowPoolExpansion, bloodParent);
        }
    }

    public GameObject SpawnBullet(Vector3 position, Quaternion rotation)
    {
        return SpawnFromPool(BULLET_POOL, position, rotation);
    }

    public GameObject SpawnSubBullet(Vector3 position, Quaternion rotation)
    {
        return SpawnFromPool(SUB_BULLET_POOL, position, rotation);
    }

    public GameObject SpawnEnemy(Vector3 position, Quaternion rotation)
    {
        return SpawnFromPool(ENEMY_POOL, position, rotation);
    }

    public GameObject SpawnBloodSplatter(Vector3 position, Quaternion rotation)
    {
        return SpawnFromPool(BLOOD_POOL, position, rotation);
    }

    private GameObject SpawnFromPool(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在！");
            return null;
        }

        return pools[poolName].Spawn(position, rotation);
    }

    public void RecycleBullet(GameObject bullet)
    {
        RecycleToPool(BULLET_POOL, bullet);
    }

    public void RecycleSubBullet(GameObject subBullet)
    {
        RecycleToPool(SUB_BULLET_POOL, subBullet);
    }

    public void RecycleEnemy(GameObject enemy)
    {
        RecycleToPool(ENEMY_POOL, enemy);
    }

    public void RecycleBloodSplatter(GameObject blood)
    {
        RecycleToPool(BLOOD_POOL, blood);
    }

    private void RecycleToPool(string poolName, GameObject obj)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在！");
            Destroy(obj);
            return;
        }

        pools[poolName].Recycle(obj);
    }

    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }
        pools.Clear();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            ClearAllPools();
            Instance = null;
        }
    }
}
