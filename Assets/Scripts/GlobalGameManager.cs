using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : BaseSingleton<GlobalGameManager>
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        //data should persist when changing scenes
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
