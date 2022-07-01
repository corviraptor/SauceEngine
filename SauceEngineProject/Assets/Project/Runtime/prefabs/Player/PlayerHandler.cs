using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public GameObject controllerObject;
    public GameObject hudObject;
    public PlayerSettings player;
    public PlayerArgs playerArgs;
    public PlayerMovement playerMovement;

    void Start(){   
        foreach (Behaviour script in controllerObject.GetComponentsInChildren<Behaviour>()){
            script.enabled = true;
        }
        foreach (Behaviour script in hudObject.GetComponentsInChildren<Behaviour>()){
            script.enabled = true;
        }

        playerArgs = new PlayerArgs(Vector3.zero, Vector3.zero, 0F, transform, playerMovement.cc, transform);

        GameEvents.current.OnPlayerUpdate += PlayerUpdate;
    }

    void OnDestroy(){   
        GameEvents.current.OnPlayerUpdate -= PlayerUpdate;
    }

    void LateUpdate(){
        if (PauseMenu.isPaused){
            return;
        }

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
