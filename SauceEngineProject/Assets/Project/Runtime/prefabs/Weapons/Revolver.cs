using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : WeaponParent
{
    public override void Inputs(PlayerArgs playerArgs){
        if(InputManager.current.attack){ PrimaryFire(playerArgs); }
        if(InputManager.current.attack2){ SecondaryFire(playerArgs); }
    }
    public override void PrimaryFire(PlayerArgs playerArgs){
        if (gunState != 0){
            // recovering, cycling the action, or reloading
            return;
        }
        if (loadedRounds == 0){
            // empty clip
            GameEvents.current.SoundCommand("HeatLimit", "Play", 0);
            return;
        }
        UnityEngine.Debug.Log("Fired Revolver! 1");
    }
    
    public override void SecondaryFire(PlayerArgs playerArgs){
        if (gunState != 0){
            // recovering, cycling the action, or reloading
            return;
        }
        if (loadedRounds == 0){
            // empty clip
            GameEvents.current.SoundCommand("HeatRecovery", "Play", 0);
            return;
        }
        UnityEngine.Debug.Log("Fired Revolver! 2");
    }

    public override void WeaponSpell(PlayerArgs playerArgs){}
    public override void Cycle(string name){}
}
