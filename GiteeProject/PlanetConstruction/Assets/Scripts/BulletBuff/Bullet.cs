using UnityEngine;
using System.Linq;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float trackingStrength = 2f; // 跟踪强度
    public float trackingRange = 5f;    // 跟踪距离
    private float damage;
    private int penetration;
    private int bounceTimes;
    private int subBulletCount;
    private float subBulletDamage;
    private Vector2 direction;

    public GameObject subBulletPrefab;

    public float dieTime = 3; // 生命周期

    public void Init(Vector2 dir, float dmg, int pen, int bounce, int subCount, float subDmg)
    {
        direction = dir.normalized;
        damage = dmg;
        penetration = pen;
        bounceTimes = bounce;
        subBulletCount = subCount;
        subBulletDamage = subDmg;
        Destroy(gameObject, dieTime); // 自动销毁时间
    }

    void Update()
    {
        Enemy target = FindClosestEnemy();

        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.transform.position);

            if (dist <= trackingRange)
            {
                Vector2 targetDir = (target.transform.position - transform.position).normalized;
                // 轻微转向目标方向
                direction = Vector2.Lerp(direction, targetDir, trackingStrength * Time.deltaTime).normalized;
            }
        }
        // 如果目标是 null，不改变方向，继续飞行
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    Enemy FindClosestEnemy()
    {
        if (EnemyManager.Instance == null || EnemyManager.Instance.enemies.Count == 0) return null;

        // 过滤掉空敌人或已死的敌人
        var validEnemies = EnemyManager.Instance.enemies
            .Where(e => e != null && e.hp > 0) // 确认敌人还活着
            .ToList();

        if (validEnemies.Count == 0) return null;

        return validEnemies
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .FirstOrDefault();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            // ✅ 无论穿透与否，都可生成副子弹
            if (bounceTimes > 0 && subBulletCount > 0)
            {
                Bounce(enemy);
            }

            // 穿透逻辑
            if (penetration > 0)
            {
                penetration--; // 保留子弹，继续飞行
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    void Bounce(Enemy hitEnemy)
    {
        var validTargets = EnemyManager.Instance.enemies
            .Where(e => e != null && e != hitEnemy && e.hp > 0) // 排除自己和已死敌人
            .OrderBy(e => Vector2.Distance(hitEnemy.transform.position, e.transform.position))
            .Take(subBulletCount)
            .ToList();

        if (validTargets.Count == 0) return;

        foreach (var target in validTargets)
        {
            Vector2 dir = (target.transform.position - hitEnemy.transform.position).normalized;
            GameObject sBulletObj = Instantiate(subBulletPrefab, hitEnemy.transform.position, Quaternion.identity);
            Bullet sBullet = sBulletObj.GetComponent<Bullet>();

            // ✅ 副子弹也会重新找最近敌人来跟踪
            sBullet.Init(dir, subBulletDamage, 0, bounceTimes - 1, subBulletCount, subBulletDamage);
        }
    }
}