using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneController : MonoBehaviour
{
    public void OnWinButton()
    {
        BattleTrigger.battleWin = true;
        BattleTrigger.battleFinished = true;

        // 卸载战斗场景
        SceneManager.UnloadSceneAsync("BattleScene");
    }

    public void OnLoseButton()
    {
        BattleTrigger.battleWin = false;
        BattleTrigger.battleFinished = true;

        // 卸载战斗场景
        SceneManager.UnloadSceneAsync("BattleScene");
    }
}