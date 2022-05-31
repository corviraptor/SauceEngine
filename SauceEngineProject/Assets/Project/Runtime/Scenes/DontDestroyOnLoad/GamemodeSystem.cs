using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamemodeSystem : MonoBehaviour
{
    public bool loadStartOnPlay;
    public static GamemodeSystem current;
    // Start is called before the first frame update
    void Awake(){
        current = this;

        if(!SceneManager.GetSceneByName("Start").isLoaded && loadStartOnPlay){
            SceneManager.LoadSceneAsync("Start", LoadSceneMode.Additive);
        }
    }

    public void switchMode(string name){
        SceneManager.UnloadSceneAsync("Start");

        if(!SceneManager.GetSceneByName(name).isLoaded){
            SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        }
    }
}
