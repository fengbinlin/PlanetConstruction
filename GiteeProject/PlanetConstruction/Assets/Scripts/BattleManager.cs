using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public List<GameObject> muzzleFlashEffectList;
    public List<GameObject> bulletEffectList;
    public List<GameObject> hitEffectList;
    // Start is called before the first frame update
    void Awake()
    {
        Instance=this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
