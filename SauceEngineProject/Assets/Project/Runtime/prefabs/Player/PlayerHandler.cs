using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public GameObject controllerObject;
    public GameObject cameraObject;
    public GameObject hudObject;
    public PlayerSettings player;
    public PlayerArgs playerArgs;
    public PlayerMovement playerMovement;

    void Start(){   
        playerArgs = new PlayerArgs(Vector3.zero, Vector3.zero, 0F, transform, playerMovement.cc, Vector3.zero, transform);

        foreach (Behaviour script in controllerObject.GetComponentsInChildren<Behaviour>()){
            if (script is IPlayerHandlerModule){
                IPlayerHandlerModule module = (IPlayerHandlerModule)script;
                module.InjectDependency(this);
                script.enabled = true;
            }
            if (!(script is AudioSource)){
                script.enabled = true;
            }
        }
        foreach (Behaviour script in hudObject.GetComponentsInChildren<Behaviour>()){
            script.enabled = true;
        }
        foreach (Behaviour script in cameraObject.GetComponentsInChildren<Behaviour>()){
            script.enabled = true;
        }

        GameEvents.current.OnPlayerUpdate += PlayerUpdate;
    }

    void OnDestroy(){   
        GameEvents.current.OnPlayerUpdate -= PlayerUpdate;
    }

    void FindPlayerMovement(){
    }

    void LateUpdate(){
        if(PauseMenu.isPaused){ return; }
        if(playerArgs == null){ return; }

        PlayerPositionUpdate(this, playerArgs);
        PlayerHudUpdate(this, playerArgs);
    }

    void PlayerUpdate(object sender){}

    public void HeatUpdate(object sender, float temperature){
        playerArgs.temperature = temperature;
    }
    
    public event Action<object, PlayerArgs> OnPlayerHudUpdate;
    public void PlayerHudUpdate(object sender, PlayerArgs playerArgs){ OnPlayerHudUpdate?.Invoke(sender, playerArgs); }

    public event Action<PlayerWeapons> OnWeaponUpdate;
    public void WeaponUpdate(PlayerWeapons sender){ OnWeaponUpdate?.Invoke(sender); }

    public event Action<object, PlayerArgs> OnPlayerPositionUpdate;
    public void PlayerPositionUpdate(object sender, PlayerArgs playerArgs){ OnPlayerPositionUpdate?.Invoke(sender, playerArgs); }

    public event Action<object, Vector3> OnBlast;
    public void Blast(object sender, Vector3 blastForceVector){ OnBlast?.Invoke(sender, blastForceVector); }

    public event Action<string, string, float> OnSoundCommand;
    public void SoundCommand(string name, string command, float value){ OnSoundCommand?.Invoke(name, command, value); }
}
