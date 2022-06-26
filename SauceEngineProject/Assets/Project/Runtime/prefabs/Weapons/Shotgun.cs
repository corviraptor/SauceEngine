using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponParent
{
    bool reloadCycleInProgress = false;

    public override void InjectDependency(WeaponManager wm){
        weaponManager = wm;
        recoveryTime = 45;
        reloadTime = 20;
        magazineSize = 8;
        loadedRounds = magazineSize;
        gunState = 0;
        clocks.Add("Shotgun", 0);
        viewmodel = weaponManager.viewmodels["Shotgun"];
    }

    void Update(){
        if (weaponManager.clocks["draw"] != 0){
            //while drawing weapon, no actions should be taken
            return;
        }

        if (loadedRounds >= magazineSize && gunState != 0){
            viewmodel.SetTrigger("ReloadEnd"); 
            gunState = 0;
            reloadCycleInProgress = false;
            reloading = false;
            return;
        }
        
        if (gunState == 0 && reloading == true){
            //only add rounds to magazine when finished reloading
            loadedRounds++;
            reloading = false;
            return;
        }

        if (reloadCycleInProgress && gunState == 0){
            Reload();
            return;
        }

        if (loadedRounds == 0 && gunState == 0){
            Reload();
            return;
        }
    }

    public override void PrimaryFire(PlayerArgs playerArgs){
        gunState = 1;
        loadedRounds--;

        reloadCycleInProgress = false;
        reloading = false;
        
        viewmodel.SetTrigger("PrimaryFire");
        base.Cycle("Shotgun", recoveryTime);
    }
    
    public override void SecondaryFire(PlayerArgs playerArgs){
        if (loadedRounds <= 1){ 
            //cant alt fire without at least 2 rounds loaded since it consumes 2 rounds
            return;
        }
        GameObject rocketInstance = ObjectPooler.SharedInstance.GetPooledObject(0);
        rocketInstance.SetActive(true);
        rocketInstance.transform.position = playerArgs.cameraTransform.position + playerArgs.cameraTransform.forward; 
        rocketInstance.transform.rotation = playerArgs.cameraTransform.rotation;

        loadedRounds -= 2;

        reloadCycleInProgress = false;
        reloading = false;

        gunState = 1;
        viewmodel.SetTrigger("SecondaryFire");
        base.Cycle("Shotgun", recoveryTime);
    }

    public override void SecondaryRelease(PlayerArgs playerArgs){}

    public override void Reload(){
        if (!reloadCycleInProgress){
            viewmodel.SetTrigger("ReloadStart"); 
            reloadCycleInProgress = true;
        }
        else {
            viewmodel.SetTrigger("ReloadLoop"); 
        }
        gunState = 2;
        reloading = true;
        base.Cycle("Shotgun", reloadTime);
    }

    public override void WeaponSpell(PlayerArgs playerArgs){}

    public override void Cycle(string name, int interval){
        base.Cycle("Shotgun", interval); 
    }
}
