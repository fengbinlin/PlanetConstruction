using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneController : MonoBehaviour
{
    public void OnWinButton()
    {
        MainRoot.instance.currentTrigger.SetBattleResult(true);
        MainRoot.instance.MainScene.gameObject.SetActive(true);

        // 卸载战斗场景
        SceneManager.UnloadSceneAsync("BattleScene");
        
    }

    public void OnLoseButton()
    {
        MainRoot.instance.currentTrigger.SetBattleResult(false);
        MainRoot.instance.MainScene.gameObject.SetActive(true);
        // 卸载战斗场景
        SceneManager.UnloadSceneAsync("BattleScene");
    }
}