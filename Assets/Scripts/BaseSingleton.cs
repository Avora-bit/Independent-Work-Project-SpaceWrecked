using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseSingleton<Template> : MonoBehaviour where Template : Component
{
    private static Template _instance;

    public static Template Instance { 
        get {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Template>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(Template).Name;
                    _instance = obj.AddComponent<Template>();
                }
            }
            return _instance; 
        }
    }

    // Start is called before the first frame update
    private void Awake() {
        if (_instance == null)
        {
            _instance = this as Template;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
