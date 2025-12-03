using UnityEngine;
using System.Collections;
using DamageNumbersPro.Demo;

public class Enemy : MonoBehaviour, IPoolable
{
    public float hp = 20f;
    public float speed = 2f;
    public float wallX = -6f;
    public float damageToPlayer = 10f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;
    
    // 新增：击中反馈的可调节参数
    [Header("击中反馈参数")]
    public float hitFeedbackDuration = 0.3f; // 击中反馈总时长
    public float maxScaleMultiplier = 1.3f;  // 最大缩放倍数
    public Color hitColor = Color.red;       // 击中时的颜色
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.3f); // 缩放曲线
    public AnimationCurve colorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 颜色变化曲线

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isKnockback = false;
    private Vector3 knockbackDirection;
    private float knockbackTimer = 0f;
    public GameObject bloodSplatterPrefab;

    private float maxHp = 20f;
    private Coroutine hitFeedbackCoroutine;
    private bool isDead = false;
    private Color originalColor; // 新增：记录原始颜色

    public void OnSpawnFromPool()
    {
        hp = maxHp;
        isKnockback = false;
        knockbackTimer = 0f;
        knockbackDirection = Vector3.zero;
        isDead = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }

        if (EnemyManager.Instance != null && !EnemyManager.Instance.enemies.Contains(this))
        {
            EnemyManager.Instance.enemies.Add(this);
        }
    }

    public void OnReturnToPool()
    {
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
            hitFeedbackCoroutine = null;
        }

        isKnockback = false;
        knockbackTimer = 0f;
        knockbackDirection = Vector3.zero;
        isDead = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.enemies.Remove(this);
        }
    }

    public void SetMaxHp(float newMaxHp)
    {
        maxHp = newMaxHp;
        hp = maxHp;
    }

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white; // 记录原始颜色
        maxHp = hp;
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (isKnockback)
        {
            knockbackTimer += Time.deltaTime;

            float knockbackProgress = knockbackTimer / knockbackDuration;
            float currentKnockbackForce = knockbackForce * (1f - knockbackProgress);

            transform.Translate(knockbackDirection * currentKnockbackForce * Time.deltaTime, Space.World);

            if (knockbackTimer >= knockbackDuration)
            {
                isKnockback = false;
                knockbackTimer = 0f;
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }

        if (transform.position.x <= wallX)
        {
            if (BattleValManager.Instance != null)
            {
                BattleValManager.Instance.TakeDamage(damageToPlayer);
            }
            Die();
        }
    }

    public void TakeDamage(float dmg, Vector3 attackDirection)
    {
        if (isDead || !gameObject.activeInHierarchy)
        {
            return;
        }

        DNP_2DDemo.instance.CreateDamageNumber(transform.position += new Vector3(0, 0, 0), dmg);
        hp -= dmg;

        ApplyKnockback(attackDirection);

        if (gameObject.activeInHierarchy)
        {
            if (hitFeedbackCoroutine != null)
            {
                StopCoroutine(hitFeedbackCoroutine);
            }
            hitFeedbackCoroutine = StartCoroutine(HitFeedback());
        }

        if (hp <= 0)
        {
            if (BattleValManager.Instance != null)
            {
                BattleValManager.Instance.GainExp(3);
            }
            Die();
        }
    }

    public void TakeDamage(float dmg)
    {
        TakeDamage(dmg, Vector3.right);
    }

    void ApplyKnockback(Vector3 direction)
    {
        isKnockback = true;
        knockbackTimer = 0f;
        knockbackDirection = direction.normalized;
    }

    IEnumerator HitFeedback()
    {
        float timer = 0f;
        
        while (timer < hitFeedbackDuration)
        {
            float progress = timer / hitFeedbackDuration;
            
            // 前半段：变大和变红
            if (progress <= 0.5f)
            {
                float scaleProgress = progress * 2f; // 映射到0-1
                float scaleValue = scaleCurve.Evaluate(scaleProgress);
                transform.localScale = originalScale * scaleValue;
                
                if (spriteRenderer != null)
                {
                    float colorProgress = colorCurve.Evaluate(scaleProgress);
                    spriteRenderer.color = Color.Lerp(originalColor, hitColor, colorProgress);
                }
            }
            // 后半段：恢复原状
            else
            {
                float recoverProgress = (progress - 0.5f) * 2f; // 映射到0-1
                float scaleValue = Mathf.Lerp(maxScaleMultiplier, 1f, recoverProgress);
                transform.localScale = originalScale * scaleValue;
                
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(hitColor, originalColor, recoverProgress);
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }

        // 确保最终状态正确
        transform.localScale = originalScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        hitFeedbackCoroutine = null;
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (bloodSplatterPrefab != null)
        {
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.SpawnBloodSplatter(transform.position, Quaternion.identity);
            }
            else
            {
                Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);
            }
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.enemies.Remove(this);
        }

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.RecycleEnemy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.enemies.Remove(this);
    }
}