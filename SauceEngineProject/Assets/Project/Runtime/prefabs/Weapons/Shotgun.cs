using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponParent
{
    public int startTime = 0;
    public int baseLoadTime = 42;
    public int lastLoadTime = 28;
    public int finish1Time = 40;
    public int finish2Time = 25;

    public override void InjectDependency(PlayerWeapons playerWeapons){
        pw = playerWeapons;
        viewmodel = pw.viewmodels["Shotgun"];

        chamberTime = 80;
        loadTime = baseLoadTime;

        fixedMag = true;
        chambered = false;
        magSize = 8;
        loadedRounds = magSize;
        roundsToLoad = 2;
        reloadStage = 0;
    }

    public override void PlayReload(){
        if (reloadStage == 0){
            viewmodel.SetTrigger("LoadStart");
            loadTime = startTime;
            return;
        }

        if (!pw.loadQueued && !chambered){
            viewmodel.SetTrigger("LoadFinish1");
            loadTime = finish1Time;
            return;
        }
        else if (!pw.loadQueued){
            viewmodel.SetTrigger("LoadFinish2");
            loadTime = finish2Time;
            return;

        }

        if (loadedRounds + roundsToLoad > magSize){
            Debug.Log(loadedRounds + roundsToLoad + " = " + loadedRounds + " + " + roundsToLoad + " > " + magSize);
            viewmodel.SetTrigger("LoadLast");
            loadTime = lastLoadTime;
            return;
        }

        // only gets here if all other "special" reload animations havent been run
        viewmodel.SetTrigger("LoadLoop");
        loadTime = baseLoadTime;
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

    public override void PrimaryRelease(PlayerArgs playerArgs){}
    
    public override void SecondaryFire(PlayerArgs playerArgs){}

    public override void SecondaryRelease(PlayerArgs playerArgs){}

    public override void WeaponSpell(PlayerArgs playerArgs){}
}