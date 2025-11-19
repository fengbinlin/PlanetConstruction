using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class Bullet : MonoBehaviour, IPoolable
{
    public float speed = 10f;
    public float trackingStrength = 2f;
    public float trackingRange = 5f;
    private float damage;
    private int penetration;
    private int bounceTimes;
    private int subBulletCount;
    private float subBulletDamage;
    private Vector2 direction;

    private Enemy lockedTarget;
    private bool hasValidTarget = false;
    private Vector2 initialDirection;

    public GameObject subBulletPrefab;
    public float dieTime = 3;

    private bool isProcessingHit = false;
    private Coroutine autoRecycleCoroutine;
    private bool isSubBullet = false;

    public void OnSpawnFromPool()
    {
    }

    public void OnReturnToPool()
    {
        if (autoRecycleCoroutine != null)
        {
            StopCoroutine(autoRecycleCoroutine);
            autoRecycleCoroutine = null;
        }

        damage = 0;
        penetration = 0;
        bounceTimes = 0;
        subBulletCount = 0;
        subBulletDamage = 0;
        direction = Vector2.zero;
        lockedTarget = null;
        hasValidTarget = false;
        isProcessingHit = false;
        isSubBullet = false;
        initialDirection = Vector2.zero;
        speed = 10f;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void InitWithData(BulletData data)
    {
        speed = data.speed;
        trackingStrength = data.trackingStrength;
        trackingRange = data.trackingRange;
        damage = data.damage;
        penetration = data.penetration;
        bounceTimes = data.bounceTimes;
        subBulletCount = data.subBulletCount;
        subBulletDamage = data.subBulletDamage;
        dieTime = data.dieTime;
        subBulletPrefab = data.subBulletPrefab;

        if (data.direction == Vector2.zero)
        {
            direction = Vector2.right;
        }
        else
        {
            direction = data.direction.normalized;
        }

        initialDirection = direction;

        if (data.lockTarget)
        {
            LockInitialTarget();
        }

        UpdateRotation();

        if (autoRecycleCoroutine != null)
        {
            StopCoroutine(autoRecycleCoroutine);
        }
        autoRecycleCoroutine = StartCoroutine(AutoRecycle());
    }

    private IEnumerator AutoRecycle()
    {
        yield return new WaitForSeconds(dieTime);
        RecycleSelf();
    }

    private void RecycleSelf()
    {
        if (ObjectPoolManager.Instance != null)
        {
            if (isSubBullet)
            {
                ObjectPoolManager.Instance.RecycleSubBullet(gameObject);
            }
            else
            {
                ObjectPoolManager.Instance.RecycleBullet(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 改动：增加 lockTarget 参数，控制是否自动找初始目标
    public void Init(Vector2 dir, float dmg, int pen, int bounce, int subCount, float subDmg, bool lockTarget = true)
    {
        if (dir == Vector2.zero)
        {
            dir = Vector2.right;
        }

        direction = dir.normalized;
        initialDirection = direction;
        damage = dmg;
        penetration = pen;
        bounceTimes = bounce;
        subBulletCount = subCount;
        subBulletDamage = subDmg;

        if (lockTarget)
        {
            LockInitialTarget();
        }

        UpdateRotation();

        if (autoRecycleCoroutine != null)
        {
            StopCoroutine(autoRecycleCoroutine);
        }
        autoRecycleCoroutine = StartCoroutine(AutoRecycle());
    }

    void Update()
    {
        if (hasValidTarget)
        {
            if (lockedTarget == null || lockedTarget.hp <= 0)
            {
                hasValidTarget = false;
                direction = Vector2.right;   // 默认方向
                UpdateRotation();
            }
            else
            {
                float dist = Vector2.Distance(transform.position, lockedTarget.transform.position);
                if (dist <= trackingRange)
                {
                    Vector2 targetDir = (lockedTarget.transform.position - transform.position);
                    if (targetDir.sqrMagnitude > 1f)
                    {
                        targetDir = targetDir.normalized;
                        direction = Vector2.Lerp(direction, targetDir, trackingStrength * Time.deltaTime).normalized;
                    }
                }
                else
                {
                    hasValidTarget = false;
                }
            }
        }

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        UpdateRotation();
    }

    void LockInitialTarget()
    {
        if (EnemyManager.Instance == null || EnemyManager.Instance.enemies.Count == 0)
        {
            hasValidTarget = false;
            return;
        }

        var validEnemies = EnemyManager.Instance.enemies
            .Where(e => e != null && e.hp > 0)
            .ToList();

        if (validEnemies.Count == 0)
        {
            hasValidTarget = false;
            return;
        }

        lockedTarget = validEnemies
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .FirstOrDefault();

        hasValidTarget = (lockedTarget != null);

        if (hasValidTarget)
        {
            Vector2 targetDir = (lockedTarget.transform.position - transform.position);
            if (targetDir.sqrMagnitude > 1f)
            {
                targetDir = targetDir.normalized;
                direction = Vector2.Lerp(direction, targetDir, 0.3f).normalized;
            }
        }
    }

    void TryFindNewTarget()
    {
        if (EnemyManager.Instance == null)
        {
            hasValidTarget = false;
            return;
        }

        var validEnemies = EnemyManager.Instance.enemies
            .Where(e => e != null && e.hp > 0)
            .ToList();

        if (validEnemies.Count == 0)
        {
            hasValidTarget = false;
            return;
        }

        lockedTarget = validEnemies
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .FirstOrDefault();

        hasValidTarget = (lockedTarget != null);
    }

    void UpdateRotation()
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isProcessingHit) return;

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null) return;

        isProcessingHit = true;

        enemy.TakeDamage(damage);

        bool shouldBounce = bounceTimes > 0 && subBulletCount > 0;
        if (shouldBounce)
        {
            Bounce(enemy);
        }

        if (penetration > 0)
        {
            penetration--;

            if (hasValidTarget && lockedTarget == enemy)
            {
                TryFindNewTarget();
            }
        }
        else
        {
            if (!shouldBounce)
            {
                RecycleSelf();
            }
        }

        isProcessingHit = false;
    }

    void Bounce(Enemy hitEnemy)
    {
        bounceTimes--;

        var validTargets = EnemyManager.Instance.enemies
            .Where(e => e != null && e != hitEnemy && e.hp > 0)
            .OrderBy(e => Vector2.Distance(hitEnemy.transform.position, e.transform.position))
            .Take(subBulletCount)
            .ToList();

        foreach (var target in validTargets)
        {
            Vector2 dir = (target.transform.position - hitEnemy.transform.position);
            if (dir.sqrMagnitude > 1f)
            {
                dir = dir.normalized;
            }
            else
            {
                dir = Vector2.right;
            }

            GameObject sBulletObj = null;
            if (ObjectPoolManager.Instance != null && ObjectPoolManager.Instance.subBulletPrefab != null)
            {
                sBulletObj = ObjectPoolManager.Instance.SpawnSubBullet(transform.position, Quaternion.identity);
            }
            else
            {
                sBulletObj = Instantiate(subBulletPrefab, transform.position, Quaternion.identity);
            }

            Scene currentScene = gameObject.scene;
            if (currentScene.IsValid() && sBulletObj.scene != currentScene)
            {
                sBulletObj.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(sBulletObj, currentScene);
            }

            Bullet sBullet = sBulletObj.GetComponent<Bullet>();
            Collider2D collider = sBulletObj.GetComponent<Collider2D>();

            if (sBullet != null)
            {
                sBullet.enabled = true;
                sBullet.isSubBullet = true;
            }
            if (collider != null) collider.enabled = true;

            sBulletObj.SetActive(true);

            sBullet.Init(dir, subBulletDamage, 0, bounceTimes, subBulletCount, subBulletDamage, false);
        }

        RecycleSelf();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = hasValidTarget ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        if (hasValidTarget && lockedTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, lockedTarget.transform.position);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
}