using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    public PlayerHandler playerHandler;
    public GameObject viewmodelObject;

    public WeaponParent heldGun;

    PlayerArgs pArgs;

    public Dictionary<string, Animator> viewmodels = new Dictionary<string, Animator>();
    Dictionary<string, WeaponParent> inventory = new Dictionary<string, WeaponParent>();

    string gunKey;
    bool hasGuns = false;
    public bool recovering = false;
    public bool ready = true;
    bool primaryReleased = false;
    bool secondaryReleased = false;
    bool currentlyReloading = false;
    bool reloadQueued = false;
    public int drawTime = 15;


    void OnEnable(){
        GameEvents.current.OnGetWeapon += GetWeapon;

        foreach (Animator a in viewmodelObject.GetComponentsInChildren<Animator>()){
            viewmodels.Add(a.gameObject.name, a);
            viewmodels[a.gameObject.name].gameObject.SetActive(false);
        }
        ready = true;
    }

    void GetWeapon(object sender, string gun){
        if (gun == null){
            return;
        }
        if (gameObject.GetComponent(Type.GetType(gun)) != null){
            return;
        }

        gunKey = gun;
        foreach (Animator a in viewmodels.Values){
            a.gameObject.SetActive(false);
        }
        viewmodels[gunKey].gameObject.SetActive(true);

        hasGuns = true;

        WeaponParent gunComponent = (WeaponParent)gameObject.AddComponent(Type.GetType(gunKey));
        gunComponent.InjectDependency(this);
        inventory.Add(gunKey, gunComponent);
    }
    
    void Update(){
        pArgs = playerHandler.playerArgs;

        if (!hasGuns){
            // none of this should run if the player doesn't have a gun
            return;
        }

        playerHandler.WeaponUpdate(this);

        TestForDrawInputs();

        if (inventory.TryGetValue(gunKey, out heldGun)){
            heldGun = inventory[gunKey];
        }

        EvaluateHeldGunInputs();
        EvaluateReload();
    }

    WeaponParent weaponTest;
    void TestForDrawInputs(){
        if (!InputManager.current.weapon0 && !InputManager.current.weapon1 && !InputManager.current.weapon2){
            //if none of the selector keys are being inputted, dont bother with this stuff 
            return;
        }

        string lastGun = gunKey;

        /* these are hard coded since i want each binding to correspond to a specific weapon no matter what, i'll probably turn this
        into something using an array instead tho to make it easier to modify and extend */
        if (InputManager.current.weapon0 && inventory.TryGetValue("Stakegun", out weaponTest) && gunKey != "Stakegun"){
            gunKey = "Stakegun";
        }

        if (InputManager.current.weapon1 && inventory.TryGetValue("Shotgun", out weaponTest ) && gunKey != "Shotgun"){
            gunKey = "Shotgun";
        }

        if (gunKey != lastGun){
            // dont draw if gun hasn't changed
            SwapGun();
        }
    }

    void SwapGun(){
        foreach (Animator a in viewmodels.Values){
            a.gameObject.SetActive(false);
        }

        StopAllCoroutines();

        viewmodels[gunKey].gameObject.SetActive(true);
        StartCoroutine(Draw());

    }
    
    void EvaluateHeldGunInputs(){
        // function won't be called if we are in the middle of drawing a weapon, if the weapon is recovering, or there is no held gun
        if (InputManager.current.attacking && heldGun.loadedRounds != 0){
            //stops reloading an internal mag gun if you press primary fire
            if (reloadQueued == true && heldGun.internalMagazine){
                reloadQueued = false;
            }

            if (ready){
                primaryReleased = false;
                heldGun.PrimaryFire(pArgs);
            }
        }

        if (InputManager.current.attack2ing && heldGun.loadedRounds != 0){
            //stops reloading an internal mag gun if you press secondary fire
            if (reloadQueued == true && heldGun.internalMagazine){
                reloadQueued = false;
            }

            if (ready){
                secondaryReleased = false;
                heldGun.SecondaryFire(pArgs);
            }
        }


        if (!InputManager.current.attack2ing && !primaryReleased && !currentlyReloading){
            heldGun.PrimaryRelease(pArgs);
            primaryReleased = true;
        }

        if (!InputManager.current.attack2ing && !secondaryReleased && !currentlyReloading){
            heldGun.SecondaryRelease(pArgs);
            secondaryReleased = true;
        }
    }  

    void EvaluateReload(){
        if (InputManager.current.reload && heldGun.loadedRounds != heldGun.magazineSize){
            //queue for reload when pressing the reload button
            reloadQueued = true;
        }

        if (heldGun.loadedRounds == heldGun.magazineSize){
            reloadQueued = false;
        }

        if (heldGun.loadedRounds == 0){
            //queue for reload automatically if no rounds are left
            reloadQueued = true;
        }

        if (recovering){
            //dont do reloads while recovering 
            return;
        }

        if (currentlyReloading){
            //dont do reloads in the middle of a reload animation
            return;
        }
        
        if (!heldGun.internalMagazine){
            ReloadExtMag();
        }
        else {
            ReloadIntMag();
        }
    }

    void ReloadExtMag(){
        if (reloadQueued && !currentlyReloading){
            //start reload
            ready = false;
            StopAllCoroutines();
            StartCoroutine(ExtReloadTimer());
            heldGun.viewmodel.SetTrigger("ReloadStart");
            return;
        }
    }

    bool reloadStarted = false;
    void ReloadIntMag(){
        if (reloadStarted && !reloadQueued){
            // plays end of reload animation when reload is unqueued
            reloadStarted = false;
            StopAllCoroutines();
            StartCoroutine(IntReloadTimer());
            heldGun.viewmodel.SetTrigger("ReloadEnd");
            return;
        }

        if (reloadQueued && !reloadStarted){
            //start of reload
            reloadStarted = true;
            ready = false;
            StopAllCoroutines();
            StartCoroutine(IntReloadTimer());
            heldGun.viewmodel.SetTrigger("ReloadStart");
            return;
        }
        
        if (reloadQueued && reloadStarted){
            reloadStarted = true;
            StopAllCoroutines();
            StartCoroutine(IntReloadTimer());
            heldGun.viewmodel.SetTrigger("ReloadLoop");
            return;
        }
    }

    //None of these coroutines should ever run concurrently, so use StopAllCoroutines() before starting one to make sure theyre not interfering!
    IEnumerator ExtReloadTimer(){
        // for external magazines
        ready = false;
        currentlyReloading = true;

        yield return new WaitForSeconds(Time.fixedDeltaTime * heldGun.reloadTime);

        currentlyReloading = false;
        UpdateAmmo();
        ready = true;
    }

    IEnumerator IntReloadTimer(){
        // for internal magazines
        ready = false;
        currentlyReloading = true;

        yield return new WaitForSeconds(Time.fixedDeltaTime * heldGun.reloadTime);

        currentlyReloading = false;

        if (!reloadStarted){
            ready = true;
        }
        else {
            UpdateAmmo();
        }
    }

    void UpdateAmmo(){
        if (!reloadQueued){
            // dont run if this is the end of reload animation
            return;
        }
        if (heldGun.loadedRounds >= heldGun.magazineSize){
            //dont add rounds if we are at or above the magazine size, this theoretically should not be called
            Debug.Log("Magazine limit reached");
            return;
        }

        if (heldGun.internalMagazine){
            heldGun.loadedRounds++;
        }
        else {
            heldGun.loadedRounds = heldGun.magazineSize;
        }
    }

    int gunInterval;
    public void Recover(int interval){
        gunInterval = interval;
        StartCoroutine(RecoveryTimer());
    }

    IEnumerator RecoveryTimer(){
        recovering = true;
        yield return new WaitForSeconds(Time.fixedDeltaTime * gunInterval);
        recovering = false;
        if (!reloadStarted){
            ready = true;
        }
    }

    IEnumerator Draw(){
        currentlyReloading = false;
        reloadStarted = false;
        reloadQueued = false;

        yield return new WaitForSeconds(Time.fixedDeltaTime * drawTime);

        // set gun to ready at end of draw time so you can fire immediately after no matter what
        ready = true;
    }
}
