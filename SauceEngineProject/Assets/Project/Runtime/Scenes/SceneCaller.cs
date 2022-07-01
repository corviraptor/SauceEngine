using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCaller : MonoBehaviour
{
    public void OnSceneCall(string name){
        GamemodeSystem.current.SwitchMode(name, gameObject.scene);
    }
}
