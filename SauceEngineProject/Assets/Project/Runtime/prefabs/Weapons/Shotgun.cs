using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponParent
{   
    public override void InjectDependency(WeaponManager wm){
        weaponManager = wm;
        loadedRounds = 8;
        gunState = 0;
        clocks.Add("Shotgun", 0);
        viewmodel = weaponManager.viewmodels["Shotgun"];
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
            GameEvents.current.SoundCommand("SlideFail", "Play", 0);
            return;
        }

        gunState = 1;
        viewmodel.SetTrigger("PrimaryFire");
        base.Cycle("Shotgun");
    }
    
    public override void SecondaryFire(PlayerArgs playerArgs){
        if (gunState != 0){
            // recovering, cycling the action, or reloading
            return;
        }
        if (loadedRounds == 0){
            // empty clip
            GameEvents.current.SoundCommand("SlideJump", "Play", 0);
            return;
        }

        GameObject rocketInstance = ObjectPooler.SharedInstance.GetPooledObject(0);
        rocketInstance.SetActive(true);
        rocketInstance.transform.position = playerArgs.cameraTransform.position + playerArgs.cameraTransform.forward; 
        rocketInstance.transform.rotation = playerArgs.cameraTransform.rotation;

        gunState = 1;
        viewmodel.SetTrigger("SecondaryFire");
        base.Cycle("Shotgun");
    }

    public override void Reload(PlayerArgs playerArgs){}

    public override void WeaponSpell(PlayerArgs playerArgs){}

    public override void Cycle(string name){
        base.Cycle("Shotgun"); 
    }
}
