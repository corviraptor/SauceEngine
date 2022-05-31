using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCaller : MonoBehaviour
{
    public void onSceneCall(string name){
        GamemodeSystem.current.switchMode(name);
    }
}
