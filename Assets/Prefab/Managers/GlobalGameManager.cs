using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameManager : BaseSingleton<GlobalGameManager>
{
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        //data should persist when changing scenes
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
