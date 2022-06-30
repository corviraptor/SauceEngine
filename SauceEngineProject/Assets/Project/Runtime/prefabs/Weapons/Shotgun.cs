using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponParent
{
    public override void InjectDependency(PlayerWeapons pw){
        internalMagazine = true;
        playerWeapons = pw;
        recoveryTime = 50;
        reloadTime = 20;
        magazineSize = 8;
        loadedRounds = magazineSize;
        viewmodel = playerWeapons.viewmodels["Shotgun"];
    }

    void Update(){
    }

    public override void PrimaryFire(PlayerArgs playerArgs){
        if (playerWeapons.ready){
            loadedRounds--;
            playerWeapons.Recover(recoveryTime);
            viewmodel.SetTrigger("PrimaryFire");
            playerWeapons.ready = false;
        }
    }

    public override void PrimaryRelease(PlayerArgs playerArgs){
    }
    
    public override void SecondaryFire(PlayerArgs playerArgs){
        if (loadedRounds <= 1){ 
            //cant alt fire without at least 2 rounds loaded since it consumes 2 rounds
            return;
        }
        if (playerWeapons.ready){
            GameObject rocketInstance = ObjectPooler.SharedInstance.GetPooledObject(0);
            rocketInstance.SetActive(true);
            rocketInstance.transform.position = playerArgs.cameraTransform.position + playerArgs.cameraTransform.forward; 
            rocketInstance.transform.rotation = playerArgs.cameraTransform.rotation;

            loadedRounds -= 2;
            playerWeapons.Recover(recoveryTime);
            viewmodel.SetTrigger("SecondaryFire");
            playerWeapons.ready = false;
        }
    }

    public override void SecondaryRelease(PlayerArgs playerArgs){}

    public override void WeaponSpell(PlayerArgs playerArgs){}
}