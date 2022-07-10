using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stakegun : WeaponParent
{   
    public float coolantDrain = 30;
    public int startTime = 11;
    public int magoutTime = 12;
    public int maginTime = 17;
    public int boltpullTime = 63;
    bool coolanting = false;
    float coolant = 100;

    public override void InjectDependency(PlayerWeapons playerWeapons){
        pw = playerWeapons;
        viewmodel = pw.viewmodels["Stakegun"];

        chamberTime = 68;
        loadTime = startTime;

        fixedMag = false;
        chambered = false;
        reloadStage = 3;
        magSize = 5;
        loadedRounds = magSize;
        roundsToLoad = magSize;
        reloadStage = 0;
    }

    public override void SetLoadTime(bool loadStarted, bool loadQueued){

        if (loadStarted == false){
            viewmodel.SetTrigger("LoadStart");
            loadTime = startTime;
            Debug.Log("Reload Started...");
            return;
        }

        // only gets here if the start reload animation finishes
        switch (reloadStage) {
            case 1:
                loadTime = magoutTime;
                viewmodel.SetTrigger("MagOut");
                break;
            case 2:
                loadTime = maginTime; 
                viewmodel.SetTrigger("MagIn");
                break;
            case 3:
                loadTime = boltpullTime; 
                viewmodel.SetTrigger("BoltPull");
                break;
            default:
                Debug.Log("Stakegun reloadStage is out of range!");
                break;
        }
    }

    void Update(){
    }

    public override void PrimaryFire(PlayerArgs playerArgs){
        if (chambered){
            chambered = false;
            loadedRounds--;
            viewmodel.SetTrigger("Attack1");
            pw.Chamber(chamberTime);
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
