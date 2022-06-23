using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public static PlayerHandler current;
    public GameObject controllerObject;
    public PlayerSettings player;
    public PlayerArgs playerArgs;
    public PlayerMovement playerMovement;

    void Start()
    {   
        current = this;

        foreach (Behaviour script in controllerObject.GetComponents<Behaviour>()){
            script.enabled = true;
        }

        playerArgs = new PlayerArgs(Vector3.zero, Vector3.zero, 0F, transform, playerMovement.cc, transform);

        GameEvents.current.OnPlayerUpdate += PlayerUpdate;
    }

    void OnDestroy()
    {   
        foreach (Behaviour script in controllerObject.GetComponents<Behaviour>()){
            script.enabled = false;
        }

        GameEvents.current.OnPlayerUpdate -= PlayerUpdate;
    }

    void LateUpdate(){
        GameEvents.current.PlayerInfo(this, playerArgs);

        PlayerPositionUpdate(this, playerArgs.transform, playerArgs.controller.height, playerArgs.controller.center);
        PlayerHudUpdate(this, playerArgs);
    }

    void PlayerUpdate(object sender){}

    public void HeatUpdate(object sender, float temperature){
        playerArgs.temperature = temperature;
    }
    
    public event Action<object, PlayerArgs> OnPlayerHudUpdate;
    public void PlayerHudUpdate(object sender, PlayerArgs playerArgs){
        if (OnPlayerHudUpdate != null){
            OnPlayerHudUpdate(sender, playerArgs);
        }
    }

    public event Action<object, Transform, float, Vector3> OnPlayerPositionUpdate;
    public void PlayerPositionUpdate(object sender, Transform pTransform, float height, Vector3 center){
        if (OnPlayerPositionUpdate != null){
            OnPlayerPositionUpdate(sender, pTransform, height, center);
        }
    }

    public event Action<object, Vector3> OnBlast;
    public void Blast(object sender, Vector3 blastForceVector){
        if (OnBlast != null){
            OnBlast(sender, blastForceVector);
        }
    }
}
