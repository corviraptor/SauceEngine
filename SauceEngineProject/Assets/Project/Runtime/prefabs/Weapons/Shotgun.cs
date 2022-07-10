using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponParent
{
    public int startTime = 0;
    public int baseLoadTime = 42;
    public int lastLoadTime = 28;
    public int finish1Time = 100;
    public int finish2Time = 25;

    public override void InjectDependency(PlayerWeapons playerWeapons){
        pw = playerWeapons;
        viewmodel = pw.viewmodels["Shotgun"];

        chamberTime = 80;
        loadTime = baseLoadTime;

        fixedMag = true;
        chambered = false;
        magSize = 6;
        loadedRounds = magSize;
        roundsToLoad = 2;
        reloadStage = 0;
    }

    public override void SetLoadTime(bool loadStarted, bool loadQueued){
        if (!loadQueued){
            viewmodel.SetTrigger("LoadFinish1");
            loadTime = finish1Time;
            return;
        }

        if (loadStarted == false){
            viewmodel.SetTrigger("LoadStart");
            loadTime = startTime;
            return;
        }

        if (loadedRounds + roundsToLoad > magSize){
            viewmodel.SetTrigger("LoadLast");
            loadTime = lastLoadTime;
            return;
        }

        // only gets here if all other "special" reload animations havent been run
        viewmodel.SetTrigger("LoadLoop");
        loadTime = baseLoadTime;
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