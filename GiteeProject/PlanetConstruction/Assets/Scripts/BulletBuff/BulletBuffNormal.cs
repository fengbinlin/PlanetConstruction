using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class BulletBuffNormal : BulletBuff
{
    public GameObject bulletPrefab;     // 主子弹预制体
    public GameObject subBulletPrefab;  // 副子弹预制体
    public ParticleSystem muzzleFlashEffect; // 枪口粒子特效
    
    public float damage = 10f;
    public float subBulletDamage = 5f;
    public int trajectoryCount = 1;     // 弹道数
    public int burstCount = 1;          // 连发数
    public int penetration = 0;         // 穿透数
    public int bounceTimes = 0;         // 可弹射次数
    public int subBulletCount = 0;      // 每次弹射发出的副子弹数

    public float fireCooldown = 0.5f;   // 射击冷却
    private float fireTimer = 0f;
    
    // 飞船大小反馈相关变量
    private Vector3 originalScale;     // 原始大小
    public float scaleMultiplier = 1.2f; // 缩放倍数
    public float scaleDuration = 0.1f;  // 缩放持续时间
    private bool isScaling = false;     // 是否正在缩放中

    void Start()
    {
        // 记录飞船的原始大小
        originalScale = transform.localScale;
    }

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

                PlayMuzzleFlash(transform.position + new Vector3(0.5f, 0, 0), rotatedDir);

                GameObject bulletObj = null;
                if (ObjectPoolManager.Instance != null)
                {
                    bulletObj = ObjectPoolManager.Instance.SpawnBullet(transform.position, Quaternion.identity);
                }
                else
                {
                    bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                }

                Scene battleScene = SceneManager.GetSceneByName("BattleScene");
                if (battleScene.IsValid())
                {
                    // bulletObj.transform.SetParent(null);
                    // SceneManager.MoveGameObjectToScene(bulletObj, battleScene);
                }

                Bullet bullet = bulletObj.GetComponent<Bullet>();
                bullet.Init(rotatedDir, damage, penetration, bounceTimes, subBulletCount, subBulletDamage);
            }
        }
        
        if (!isScaling)
        {
            StartCoroutine(ScaleFeedback());
        }
    }

    // 播放枪口粒子特效的方法
    void PlayMuzzleFlash(Vector3 position, Vector3 direction)
    {
        if (muzzleFlashEffect != null)
        {
            // 实例化粒子特效
            ParticleSystem muzzleFlash = Instantiate(muzzleFlashEffect, position, Quaternion.identity);
            
            // 设置粒子特效的旋转方向与子弹发射方向一致
            muzzleFlash.transform.rotation = Quaternion.LookRotation(direction);
            
            // 播放粒子特效
            muzzleFlash.Play();
            
            // 粒子特效播放完成后自动销毁
            Destroy(muzzleFlash.gameObject, muzzleFlash.main.duration);
        }
    }
    
    // 飞船大小反馈协程
    System.Collections.IEnumerator ScaleFeedback()
    {
        isScaling = true;
        
        float timer = 0f;
        Vector3 targetScale = originalScale * scaleMultiplier;
        
        // 放大阶段
        while (timer < scaleDuration / 2f)
        {
            timer += Time.deltaTime;
            float progress = timer / (scaleDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        // 确保达到目标大小
        transform.localScale = targetScale;
        
        // 恢复阶段
        timer = 0f;
        while (timer < scaleDuration / 2f)
        {
            timer += Time.deltaTime;
            float progress = timer / (scaleDuration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        // 确保恢复原始大小
        transform.localScale = originalScale;
        isScaling = false;
    }
}