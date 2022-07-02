using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake(){
        current = this;
    }
    
    public event Action<object> OnPlayerUpdate;
    public void PlayerUpdate(object sender){ OnPlayerUpdate?.Invoke(sender); }
    
    public event Action<object, float> OnHeatPlayer;
    public void HeatPlayer(object sender, float amount){ OnHeatPlayer?.Invoke(sender, amount); }

    public event Action<object, float> OnSetPlayerHeat;
    public void SetPlayerHeat(object sender, float value){ OnSetPlayerHeat?.Invoke(sender, value); }

    public event Action<object, string> OnGetWeapon;
    public void GetWeapon(object sender, string gun){ OnGetWeapon?.Invoke(sender, gun); }

    public event Action<string, string, float> OnSoundCommand;
    public void SoundCommand(string name, string command, float value){ OnSoundCommand?.Invoke(name, command, value); }
}
