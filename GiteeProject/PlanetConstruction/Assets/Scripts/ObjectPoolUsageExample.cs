using UnityEngine;

public class ObjectPoolUsageExample : MonoBehaviour
{
    public BulletData defaultBulletData;

    void Start()
    {
        if (defaultBulletData == null)
        {
            defaultBulletData = BulletData.CreateDefault();
        }
    }

    public void SpawnBulletExample()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager 未初始化！");
            return;
        }

        GameObject bulletObj = ObjectPoolManager.Instance.SpawnBullet(
            transform.position,
            Quaternion.identity
        );

        if (bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                BulletData clonedData = defaultBulletData.Clone();
                clonedData.direction = Vector2.right;
                bullet.InitWithData(clonedData);
            }
        }
    }

    public void SpawnBulletWithInitExample()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager 未初始化！");
            return;
        }

        GameObject bulletObj = ObjectPoolManager.Instance.SpawnBullet(
            transform.position,
            Quaternion.identity
        );

        if (bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Init(Vector2.right, 10f, 2, 1, 3, 5f, true);
            }
        }
    }

    public void SpawnEnemyExample()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager 未初始化！");
            return;
        }

        GameObject enemyObj = ObjectPoolManager.Instance.SpawnEnemy(
            new Vector3(10f, 0f, 0f),
            Quaternion.identity
        );

        if (enemyObj != null)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetMaxHp(50f);
            }
        }
    }
}
