using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    public string battleSceneName = "BattleScene"; // 战斗场景名称
    public GameObject obstacle; // 战斗胜利后要删除的阻挡物

    // 用于记录战斗结果
    public static bool battleWin = false;
    public static bool battleFinished = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 进入战斗，加载战斗场景（叠加加载）
            SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
        }
    }

    void Update()
    {
        // 检查战斗是否结束
        if (battleFinished)
        {
            battleFinished = false; // 重置状态

            if (battleWin)
            {
                Debug.Log("战斗胜利，移除障碍物");
                if (obstacle != null)
                {
                    Destroy(obstacle);
                }
            }
            else
            {
                Debug.Log("战斗失败，障碍物保留");
            }
        }
    }
}