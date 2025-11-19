using UnityEngine;

[System.Serializable]
public class BulletData
{
    public float speed = 10f;
    public float trackingStrength = 2f;
    public float trackingRange = 5f;
    public float damage;
    public int penetration;
    public int bounceTimes;
    public int subBulletCount;
    public float subBulletDamage;
    public Vector2 direction;
    public bool lockTarget = true;
    public float dieTime = 3f;
    public GameObject subBulletPrefab;

    public BulletData Clone()
    {
        BulletData clone = new BulletData
        {
            speed = this.speed,
            trackingStrength = this.trackingStrength,
            trackingRange = this.trackingRange,
            damage = this.damage,
            penetration = this.penetration,
            bounceTimes = this.bounceTimes,
            subBulletCount = this.subBulletCount,
            subBulletDamage = this.subBulletDamage,
            direction = this.direction,
            lockTarget = this.lockTarget,
            dieTime = this.dieTime,
            subBulletPrefab = this.subBulletPrefab
        };
        return clone;
    }

    public static BulletData CreateDefault()
    {
        return new BulletData
        {
            speed = 10f,
            trackingStrength = 2f,
            trackingRange = 5f,
            damage = 10f,
            penetration = 0,
            bounceTimes = 0,
            subBulletCount = 0,
            subBulletDamage = 5f,
            direction = Vector2.right,
            lockTarget = true,
            dieTime = 3f
        };
    }
}
