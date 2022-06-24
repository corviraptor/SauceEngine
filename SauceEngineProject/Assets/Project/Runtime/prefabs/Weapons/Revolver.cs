using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : WeaponParent
{
    public override void InjectDependency(WeaponManager wm){
        weaponManager = wm;
    }

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
    }

    public override void Reload(PlayerArgs playerArgs){}

    public override void WeaponSpell(PlayerArgs playerArgs){}

    public override void Cycle(string name){}
}
