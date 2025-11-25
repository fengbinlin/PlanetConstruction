using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class BulletBuffNormal : BulletBuff
{
    [Header("基础属性")]
    public GameObject bulletPrefab;
    public GameObject subBulletPrefab;
    public ParticleSystem muzzleFlashEffect;
    
    [Header("伤害相关")]
    public float damage = 10f;
    public float subBulletDamage = 5f;
    public float critChance = 0f;        // 暴击率
    public float critMultiplier = 2f;    // 暴击倍率
    
    [Header("射击相关")]
    public int trajectoryCount = 1;
    public int burstCount = 1;
    public float fireCooldown = 0.5f;
    public float bulletSpeed = 10f;
    public float burstInterval = 0.1f;   // 连发间隔
    
    [Header("特殊效果")]
    public int penetration = 0;
    public int bounceTimes = 0;
    public int subBulletCount = 0;
    
    [Header("连射相关")]
    public bool hasRapidFire = false;    // 是否连射
    public int rapidFireCount = 0;       // 连射数量
    
    [Header("散射相关")]
    public bool hasSpreadShot = false;   // 是否散射
    public float spreadAngle = 0f;       // 散射角度
    
    private float fireTimer = 0f;
    private Vector3 originalScale;
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.1f;
    private bool isScaling = false;
    private bool isBurstFiring = false;  // 是否正在连发中

    // 记录已获得的升级ID
    public HashSet<string> acquiredUpgrades = new HashSet<string>();

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        fireTimer += Time.deltaTime;
        
        // 如果不在连发状态且冷却时间到，开始射击
        if (!isBurstFiring && fireTimer >= fireCooldown)
        {
            Enemy target = FindClosestEnemy();
            if (target != null)
            {
                StartCoroutine(BurstFire(target));
                fireTimer = 0f;
            }
        }
    }

    Enemy FindClosestEnemy()
    {
        if (EnemyManager.Instance == null) return null;
        var validEnemies = EnemyManager.Instance.enemies.Where(e => e != null).ToList();
        if (validEnemies.Count == 0) return null;
        return validEnemies.OrderBy(e => Vector2.Distance(transform.position, e.transform.position)).FirstOrDefault();
    }

    System.Collections.IEnumerator BurstFire(Enemy target)
    {
        isBurstFiring = true;
        
        Vector2 baseDir = (target.transform.position - transform.position).normalized;
        int totalShots = hasRapidFire ? rapidFireCount + 1 : 1;
        
        for (int shot = 0; shot < totalShots; shot++)
        {
            // 每次射击都重新计算方向（目标可能移动）
            if (target != null)
            {
                baseDir = (target.transform.position - transform.position).normalized;
            }
            
            FireSingleShot(baseDir);
            
            // 如果不是最后一次射击，等待间隔
            if (shot < totalShots - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }
        
        isBurstFiring = false;
        
        // 大小反馈
        if (!isScaling)
        {
            StartCoroutine(ScaleFeedback());
        }
    }

    void FireSingleShot(Vector2 baseDir)
    {
        // 处理散射
        if (hasSpreadShot && trajectoryCount > 1)
        {
            FireSpreadShot(baseDir);
        }
        else
        {
            FireStandardShot(baseDir);
        }
    }

    void FireStandardShot(Vector2 baseDir)
    {
        for (int i = 0; i < trajectoryCount; i++)
        {
            Vector2 shotDir = baseDir;
            
            // 多弹道角度偏移
            if (trajectoryCount > 1)
            {
                float angleOffset = ((i % 2 == 0 ? 1 : -1) * (5 * ((i + 1) / 2)));
                shotDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
            }

            CreateBullet(shotDir);
        }
    }

    void FireSpreadShot(Vector2 baseDir)
    {
        float totalAngle = spreadAngle * (trajectoryCount - 1);
        float startAngle = -totalAngle / 2f;
        
        for (int i = 0; i < trajectoryCount; i++)
        {
            float angle = startAngle + i * spreadAngle;
            Vector2 shotDir = Quaternion.Euler(0, 0, angle) * baseDir;
            CreateBullet(shotDir);
        }
    }

    void CreateBullet(Vector2 direction)
    {
        PlayMuzzleFlash(transform.position + (Vector3)direction * 0.5f, direction);

        GameObject bulletObj = ObjectPoolManager.Instance?.SpawnBullet(transform.position, Quaternion.identity) ?? 
                              Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        float finalDamage = damage;
        
        // 暴击判断
        if (Random.value < critChance)
        {
            finalDamage *= critMultiplier;
        }
        
        bullet.Init(direction, finalDamage, penetration, bounceTimes, subBulletCount, subBulletDamage);
    }

    void PlayMuzzleFlash(Vector3 position, Vector3 direction)
    {
        if (muzzleFlashEffect != null)
        {
            ParticleSystem muzzleFlash = Instantiate(muzzleFlashEffect, position, Quaternion.identity);
            muzzleFlash.transform.rotation = Quaternion.LookRotation(direction);
            muzzleFlash.Play();
            Destroy(muzzleFlash.gameObject, muzzleFlash.main.duration);
        }
    }
    
    System.Collections.IEnumerator ScaleFeedback()
    {
        isScaling = true;
        float timer = 0f;
        Vector3 targetScale = originalScale * scaleMultiplier;
        
        while (timer < scaleDuration / 2f)
        {
            timer += Time.deltaTime;
            float progress = timer / (scaleDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        transform.localScale = targetScale;
        
        timer = 0f;
        while (timer < scaleDuration / 2f)
        {
            timer += Time.deltaTime;
            float progress = timer / (scaleDuration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
        isScaling = false;
    }
}