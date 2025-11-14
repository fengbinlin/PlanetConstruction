using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BulletBuffNormal : BulletBuff
{
    public GameObject bulletPrefab;     // 主子弹预制体
    public GameObject subBulletPrefab;  // 副子弹预制体

    public float damage = 10f;
    public float subBulletDamage = 5f;
    public int trajectoryCount = 1;     // 弹道数
    public int burstCount = 1;          // 连发数
    public int penetration = 0;         // 穿透数
    public int bounceTimes = 0;         // 可弹射次数
    public int subBulletCount = 0;      // 每次弹射发出的副子弹数

    public float fireCooldown = 0.5f;   // 射击冷却
    private float fireTimer = 0f;

    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireCooldown)
        {
            Enemy target = FindClosestEnemy();
            if (target != null)
            {
                Fire(target);
                fireTimer = 0f;
            }
        }
    }

    Enemy FindClosestEnemy()
    {
        if (EnemyManager.Instance == null) return null;

        // 过滤掉空的敌人引用
        var validEnemies = EnemyManager.Instance.enemies.Where(e => e != null).ToList();
        if (validEnemies.Count == 0) return null;

        return validEnemies
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .FirstOrDefault();
    }

    void Fire(Enemy target)
    {
        Vector2 dir = (target.transform.position - transform.position).normalized;

        for (int b = 0; b < burstCount; b++)
        {
            for (int i = 0; i < trajectoryCount; i++)
            {
                float angleOffset = ((i % 2 == 0 ? 1 : -1) * (5 * ((i + 1) / 2)));
                Vector2 rotatedDir = Quaternion.Euler(0, 0, angleOffset) * dir;

                GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                Bullet bullet = bulletObj.GetComponent<Bullet>();
                bullet.Init(rotatedDir, damage, penetration, bounceTimes, subBulletCount, subBulletDamage);
            }
        }
    }
}