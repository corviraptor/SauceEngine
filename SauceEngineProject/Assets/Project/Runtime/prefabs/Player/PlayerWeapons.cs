using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    public PlayerHandler playerHandler;
    public GameObject viewmodelObject;
    FirstPersonActions.PlayerActions input => InputManager.current.input;
    public WeaponParent gun;
    PlayerArgs pArgs;

    public Dictionary<string, Animator> viewmodels = new Dictionary<string, Animator>();
    Dictionary<string, WeaponParent> inventory = new Dictionary<string, WeaponParent>();

    bool actioning = false;
    bool loadQueued = false;
    bool loading = false;
    bool loadStarted = false;
    bool hasAGun = false;
    bool primaryReleased = true;
    bool secondaryReleased = true;
    string gunKey;
    int drawTime = 25;


    void OnEnable(){
        GameEvents.current.OnGetWeapon += GetWeapon;
        InputManager.current.OnPressButtons += OnPressButtons;

        foreach (Animator a in viewmodelObject.GetComponentsInChildren<Animator>()){
            viewmodels.Add(a.gameObject.name, a);
            viewmodels[a.gameObject.name].gameObject.SetActive(false);
        }
    }

    void OnDestroy(){
        GameEvents.current.OnGetWeapon -= GetWeapon;
        InputManager.current.OnPressButtons -= OnPressButtons;
    }

    WeaponParent weaponTest;
    void OnPressButtons(Dictionary<string, bool> buttons){
        // none of this should run if the player doesn't have a gun
        if (hasAGun == false){ return; }

        string lastGun = gunKey;

        /* these are hard coded since i want each binding to correspond to a specific gun no matter what, i'll probably turn this
        into something using an array instead tho to make it easier to modify and extend */
        if (buttons["weapon0"] && inventory.TryGetValue("Stakegun", out weaponTest) && gunKey != "Stakegun"){
            gunKey = "Stakegun";
        }

        if (buttons["weapon1"] && inventory.TryGetValue("Shotgun", out weaponTest ) && gunKey != "Shotgun"){
            gunKey = "Shotgun";
        }

        if (gunKey != lastGun){
            // dont draw if gun hasn't changed
            SwapGun();
        }
    }

    void GetWeapon(object sender, string g){
        gunKey = g;
        if (gunKey == null){ return; }
        if (gameObject.GetComponent(Type.GetType(gunKey)) != null){ return; }

        WeaponParent gunComponent = (WeaponParent)gameObject.AddComponent(Type.GetType(gunKey));
        gunComponent.InjectDependency(this);
        inventory.Add(gunKey, gunComponent);

        SwapGun();
        hasAGun = true;
    }
    
    void Update(){
        pArgs = playerHandler.playerArgs;

        if (hasAGun == false){ return; }

        playerHandler.WeaponUpdate(this);

        if (inventory.TryGetValue(gunKey, out gun)){
            gun = inventory[gunKey];
        }

        EvaluateGunInputs();

        if (input.Reload.ReadValue<float>() != 0 && gun.loadedRounds != gun.magSize){
            //queue for reload when pressing the reload button
            loadQueued = true;
        }

        if (gun.loadedRounds == 0){
            //queue for reload automatically if no rounds are left
            loadQueued = true;
        }

        if (gun.reloadStage != 0){
            //queue for reload automatically if we're in the middle of a reload with a fixed mag gun
            loadQueued = true;
        }

        if (loadQueued && !actioning && !loading){
            StopAllCoroutines();
            StartCoroutine(Reload());
        }
    }

    void SwapGun(){
        foreach (Animator a in viewmodels.Values){
            a.gameObject.SetActive(false);
        }
        gun = inventory[gunKey];
        viewmodels[gunKey].gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(Draw());
    }

    IEnumerator Draw(){
        // reset fuckin everything
        loading = false;
        loadStarted = false;
        primaryReleased = false;
        secondaryReleased = false;
        gun.chambered = false;
        if (gun.fixedMag){
            // dont bother with reload stage stuff if gun has fixed mag
            gun.reloadStage = 0;
        }

        // round is chambered during draw
        actioning = true;

        yield return new WaitForSeconds(drawTime / 60F);

        // set gun to chambered at end of draw time so you can fire immediately after no matter what
        actioning = false;
        gun.chambered = true;
        StopAllCoroutines();
    }
    
    void EvaluateGunInputs(){
        // function won't be called if we are in the middle of drawing a gun, if the gun is actioning, or there is no held gun
        if (input.PrimaryFire.ReadValue<float>() != 0 && gun.loadedRounds != 0){
            // stops reloading a fixed mag gun if you press primary fire
            if (loadQueued == true && gun.fixedMag){
                loadQueued = false;
            }

            primaryReleased = false;

            if (actioning || loadStarted){ return; }

            gun.PrimaryFire(pArgs);
        }

        if (input.SecondaryFire.ReadValue<float>() != 0){
            // stops reloading a fixed mag gun if you press secondary fire
            if (loadQueued == true && gun.fixedMag){
                loadQueued = false;
            }
            
            secondaryReleased = false;

            if (actioning || loadStarted){ return; }

            gun.SecondaryFire(pArgs);
        }

        if (input.PrimaryFire.ReadValue<float>() == 0 && !primaryReleased && !loading){
            primaryReleased = true;

            if (actioning || loadStarted){ return; }

            gun.PrimaryRelease(pArgs);
        }

        if (input.SecondaryFire.ReadValue<float>() == 0 && !secondaryReleased && !loading){
            secondaryReleased = true;

            if (actioning || loadStarted){ return; }

            gun.SecondaryRelease(pArgs);
        }
    }  

    IEnumerator Reload(){
        gun.SetLoadTime(loadStarted, loadQueued);
        loading = true;

        yield return new WaitForSeconds(gun.loadTime / 60F);

        while (loading && gun.fixedMag){ //fixed mag
            loading = false;

            if (loadStarted && gun.loadedRounds + gun.roundsToLoad <= gun.magSize){
                gun.loadedRounds += gun.roundsToLoad;
            }
            else if (gun.loadedRounds + gun.roundsToLoad > gun.magSize){
                gun.loadedRounds = gun.magSize;
            }


            if (loadQueued){
                TestReloadStateChangeFixed();
            }
            else {
                loadStarted = false;
            }
        }

        while (loading && !gun.fixedMag){ //non-fixed mag
            loading = false;

            gun.reloadStage++;
            Debug.Log("Incremented reloadStage: " + gun.reloadStage);
            if (!loadStarted){
                loadStarted = true;
            }

            if (gun.reloadStage == 4){
                gun.loadedRounds = gun.magSize;
                gun.reloadStage = 0;
                loadStarted = false;
                loadQueued = false;
            }
        }
    }

    void TestReloadStateChangeFixed(){
        if (!loadStarted){
            loadStarted = true;
        }
        else if (gun.loadedRounds >= gun.magSize){
            loadQueued = false;
            StopAllCoroutines();
            StartCoroutine(Reload()); // start last cycle to close the weapon
        }
    }

    int gunInterval;
    public void Chamber(int interval){
        gunInterval = interval;
        StartCoroutine(ChamberTimer());
    }

    IEnumerator ChamberTimer(){
        actioning = true;
        Debug.Log("Chambering started");

        yield return new WaitForSeconds(gunInterval / 60F);

        Debug.Log("Chambering Finished");

        actioning = false;
        gun.chambered = true;
    }

    public void PlayViewmodelAnimation (string name){
        viewmodels[gunKey].SetTrigger(name);
    }
}