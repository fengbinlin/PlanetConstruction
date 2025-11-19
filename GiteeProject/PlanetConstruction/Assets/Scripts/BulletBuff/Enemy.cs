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

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isKnockback = false;
    private Vector3 knockbackDirection;
    private float knockbackTimer = 0f;
    public GameObject bloodSplatterPrefab;

    private float maxHp = 20f;
    private Coroutine hitFeedbackCoroutine;
    private bool isDead = false;
    public void OnSpawnFromPool()
    {
        hp = maxHp;
        isKnockback = false;
        knockbackTimer = 0f;
        knockbackDirection = Vector3.zero;
        isDead = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
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
            spriteRenderer.color = Color.white;
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
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        transform.localScale = originalScale * 1.5f;

        yield return new WaitForSeconds(0.2f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        transform.localScale = originalScale;
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