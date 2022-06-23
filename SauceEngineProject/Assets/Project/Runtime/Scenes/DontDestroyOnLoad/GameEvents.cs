using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;
    public EventSettings eventSettings;

    private void Awake(){
        current = this;
    }

    public void inputUpdate(object sender, float[] a, bool[] b){
    }

    public void PlayerInfo(object sender, PlayerArgs player){
    }
    
    public event Action<object> OnPlayerUpdate;
    public void PlayerUpdate(object sender){
        if (OnPlayerUpdate != null){
            OnPlayerUpdate(sender);
        }
    }
    
    public event Action<object, string> OnHeatPlayer;
    public void HeatPlayer(object sender, string name){
        if (OnHeatPlayer != null){
            OnHeatPlayer(sender, name);
        }
    }

    public event Action<object, string> OnGetWeapon;
    public void GetWeapon(object sender, string gun){
        if (OnGetWeapon != null){
            OnGetWeapon(sender, gun);
        }
    }

    // SOUNDS
    public event Action<string, string, float> OnSoundCommand;
    public void SoundCommand(string name, string command, float value){
        if (OnSoundCommand != null){
            OnSoundCommand(name, command, value);
        }
    }
}
