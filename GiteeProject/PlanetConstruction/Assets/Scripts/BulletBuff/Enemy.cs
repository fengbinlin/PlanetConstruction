using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float hp = 20f;
    public float speed = 2f;
    public float wallX = -6f; // 保护墙位置

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x <= wallX)
        {
            // 到达墙逻辑
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        EnemyManager.Instance.enemies.Remove(this);
        Destroy(gameObject);
    }
    void OnDestroy()
{
    if (EnemyManager.Instance != null)
        EnemyManager.Instance.enemies.Remove(this);
}
}