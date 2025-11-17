using UnityEngine;

public class UpgradeCard
{
    public string title;
    public string desc;
    public System.Action<BulletBuffNormal> applyEffect;

    public UpgradeCard(string t, string d, System.Action<BulletBuffNormal> effect)
    {
        title = t;
        desc = d;
        applyEffect = effect;
    }

    public void Apply(BulletBuffNormal buff)
    {
        if (applyEffect != null) applyEffect.Invoke(buff);
    }
}

public static class UpgradeCardDatabase
{
    public static System.Collections.Generic.List<UpgradeCard> GetAllCards()
    {
        var list = new System.Collections.Generic.List<UpgradeCard>();

        list.Add(new UpgradeCard("增强伤害", "主子弹伤害+5", (buff) => { buff.damage += 5f; }));
        list.Add(new UpgradeCard("增加弹道", "弹道数+1", (buff) => { buff.trajectoryCount += 1; }));
        list.Add(new UpgradeCard("加快射速", "冷却时间减少20%", (buff) => { buff.fireCooldown *= 0.8f; }));
        list.Add(new UpgradeCard("弹射强化", "弹射次数+1", (buff) => { buff.bounceTimes += 1; }));
        list.Add(new UpgradeCard("副子弹数量增加", "副子弹数量+1", (buff) => { buff.subBulletCount += 1; }));
        list.Add(new UpgradeCard("副子弹伤害提升", "副子弹伤害+3", (buff) => { buff.subBulletDamage += 3f; }));

        return list;
    }
}