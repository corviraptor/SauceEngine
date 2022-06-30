using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stakegun : WeaponParent
{   
    float coolant = 100;
    public override void InjectDependency(PlayerWeapons pw){
        internalMagazine = false;
        playerWeapons = pw;
        recoveryTime = 65;
        reloadTime = 80;
        magazineSize = 6;
        loadedRounds = magazineSize;
        viewmodel = playerWeapons.viewmodels["Stakegun"];
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
    }

    public override void SecondaryRelease(PlayerArgs playerArgs){
    }

    public override void WeaponSpell(PlayerArgs playerArgs){
    }
}
