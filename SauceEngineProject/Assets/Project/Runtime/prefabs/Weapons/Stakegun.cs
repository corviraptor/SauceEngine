using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stakegun : WeaponParent
{   
    public float coolantDrain = 30;

    bool coolanting = false;
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
        if (coolanting == false){
            coolant += coolantDrain * Time.deltaTime / 4;
        }
    }

    public override void PrimaryFire(PlayerArgs playerArgs){
        if (coolant < 20){
            Debug.Log("Too little coolant!");
            return;
        }
        if (!playerWeapons.ready || coolanting){
            return;
        }
        loadedRounds--;
        playerWeapons.Recover(recoveryTime);
        viewmodel.SetTrigger("PrimaryFire");
        playerWeapons.ready = false;
    }

    public override void PrimaryRelease(PlayerArgs playerArgs){
    }

    public override void SecondaryFire(PlayerArgs playerArgs){
        if (!playerWeapons.ready){
            return;
        }

        if (!coolanting){
            coolanting = true;
            viewmodel.SetTrigger("SecondaryFire");
        }
        if (coolant >= 0){
            coolant -= coolantDrain * Time.deltaTime;
            Debug.Log(coolant);
        }
        if (coolanting){
            viewmodel.SetTrigger("SecondaryLoop");
        }
    }

    public override void SecondaryRelease(PlayerArgs playerArgs){
        if (coolanting){
            coolanting = false;
            viewmodel.SetTrigger("SecondaryRelease");
        }
    }

    public override void WeaponSpell(PlayerArgs playerArgs){
    }
}
