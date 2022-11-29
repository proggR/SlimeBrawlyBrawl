using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      StartCoroutine("SplashSwitch");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SplashSwitch(){
      yield return new WaitForSeconds(3.4f);
      // Get count of loaded Scenes and last index
      var lastSceneIndex = SceneManager.sceneCount - 1;
       
      // Get last Scene by index in all loaded Scenes
      var lastLoadedScene = SceneManager.GetSceneAt(lastSceneIndex);   
      SceneManager.LoadScene("InfiniteRunScene");
      // Unload Splash Scene
      SceneManager.UnloadSceneAsync(lastLoadedScene);
    }

}
