using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneController : MonoBehaviour
{
    public void OnWinButton()
    {
        Time.timeScale = 1f;
        MainRoot.instance.currentTrigger.SetBattleResult(true);
        MainRoot.instance.MainScene.gameObject.SetActive(true);

        // 卸载战斗场景
        SceneManager.UnloadSceneAsync("BattleScene");
        
    }

    public void OnLoseButton()
    {
        Time.timeScale = 1f;
        MainRoot.instance.currentTrigger.SetBattleResult(false);
        MainRoot.instance.MainScene.gameObject.SetActive(true);
        // 卸载战斗场景
        SceneManager.UnloadSceneAsync("BattleScene");
    }
}