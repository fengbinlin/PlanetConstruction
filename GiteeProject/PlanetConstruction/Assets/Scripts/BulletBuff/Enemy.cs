using UnityEngine;
using System.Collections;
using DamageNumbersPro.Demo;

public class Enemy : MonoBehaviour
{
    public float hp = 20f;
    public float speed = 2f;
    public float wallX = -6f; // 保护墙位置
    public float damageToPlayer = 10f; // 到达墙时对玩家伤害

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x <= wallX)
        {
            // 到达墙壁，对玩家造成伤害
            if (BattleValManager.Instance != null)
            {
                BattleValManager.Instance.TakeDamage(damageToPlayer);
            }
            Die();
        }
    }

    public void TakeDamage(float dmg)
    {
        DNP_2DDemo.instance.CreateDamageNumber(transform.position+=new Vector3(0,0,0),dmg);
        hp -= dmg;

        // 调用受击反馈
        StartCoroutine(HitFeedback());

        if (hp <= 0)
        {
            if (BattleValManager.Instance != null)
            {
                BattleValManager.Instance.GainExp(3); // 每个敌人给3经验
            }
            Die();
        }
    }

    IEnumerator HitFeedback()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // 变白
        }
        transform.localScale = originalScale * 1.5f; // 放大

        yield return new WaitForSeconds(0.2f); // 停留时间

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; // 恢复成红色（或者其他默认颜色）
        }
        transform.localScale = originalScale; // 恢复大小
    }

    void Die()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.enemies.Remove(this);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.enemies.Remove(this);
    }
}