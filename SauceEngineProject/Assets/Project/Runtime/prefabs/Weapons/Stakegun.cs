using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stakegun : WeaponParent
{   
    public bool coolanting = false;
    public float coolant = 100;
    public override void InjectDependency(WeaponManager wm){
        weaponManager = wm;
        recoveryTime = 60;
        reloadTime = 80;
        magazineSize = 6;
        loadedRounds = magazineSize;
        gunState = 0;
        clocks.Add("Stakegun", 0);
        viewmodel = weaponManager.viewmodels["Stakegun"];
    }

    void Update(){
        
        if (coolant <= 100 && !coolanting){
            coolant += Time.deltaTime * 30;
        }
        
        if (gunState == 0 && reloading == true){
            //only add rounds to magazine when finished reloading
            loadedRounds = magazineSize;
            reloading = false;
            return;
        }
        if (loadedRounds == 0 && gunState == 0){
            Reload();
            return;
        }
    }

    public override void PrimaryFire(PlayerArgs playerArgs){
        Debug.Log("FIRED");
        gunState = 1;
        loadedRounds--;
        viewmodel.SetTrigger("PrimaryFire");
        base.Cycle("Stakegun", recoveryTime);
    }
    
    public override void SecondaryFire(PlayerArgs playerArgs){
        if (coolant >= 0 && gunState != 2){
            coolant -= Time.deltaTime * 60;
            Debug.Log(coolant);
            if (!coolanting){
                coolanting = true;
                viewmodel.SetTrigger("SecondaryFire");
            }
            else {
                viewmodel.SetTrigger("SecondaryLoop");
            }
        }
    }

    public override void SecondaryRelease(PlayerArgs playerArgs){
        coolanting = false;
        gunState = 0;
        viewmodel.SetTrigger("SecondaryRelease");
    }

    public override void Reload(){
        gunState = 2;
        reloading = true;
        viewmodel.SetTrigger("ReloadStart");
        base.Cycle("Stakegun", reloadTime);
    }

    public override void WeaponSpell(PlayerArgs playerArgs){}

    public override void Cycle(string name, int interval){
        base.Cycle("Stakegun", interval); 
    }
}
