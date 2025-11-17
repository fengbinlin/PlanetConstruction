using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    public string battleSceneName = "BattleScene"; // 战斗场景名称
    public GameObject obstacle; // 战斗胜利后要删除的阻挡物
    public GameObject playerpos;

    private bool battleWin = false;
    private bool battleFinished = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.transform.position = playerpos.transform.position;

            // 记录这个关卡是当前战斗触发点
            MainRoot.instance.currentTrigger = this;

            // 进入战斗场景（叠加加载）
            SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
            MainRoot.instance.MainScene.gameObject.SetActive(false);
        }
    }

    public void SetBattleResult(bool win)
    {
        battleWin = win;
        battleFinished = true;
    }

    void Update()
    {
        if (battleFinished)
        {
            battleFinished = false;

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