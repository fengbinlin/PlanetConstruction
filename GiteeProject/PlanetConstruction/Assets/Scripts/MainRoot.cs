using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainRoot : MonoBehaviour
{
    public BattleTrigger currentTrigger; // 当前正在战斗的Trigger
        public GameObject MainScene;
    public static MainRoot instance;
    // Start is called before the first frame update
    void Start()
    {
        instance=this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
