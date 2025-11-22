using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    public GameObject TechnologyLabUI;
    // Start is called before the first frame update
    void Awake()
    {
        instance=this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleTechnologyLabUI()
    {
        TechnologyLabUI.gameObject.SetActive(!TechnologyLabUI.gameObject.activeInHierarchy);
    }
}
