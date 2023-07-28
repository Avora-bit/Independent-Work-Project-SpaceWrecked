using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashLoad : MonoBehaviour
{
    private float elapsedTime = 5f;
    public string targetScene = "Scene_Menu";

    // Update is called once per frame
    void Update()
    {
        elapsedTime -= Time.deltaTime;
        if (elapsedTime <= 0)
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}
