using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{      
    public PlayerHandler playerHandler;
    public GameObject viewmodelObject;

    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public Dictionary<string, Animator> viewmodels = new Dictionary<string, Animator>();
    Dictionary<string, WeaponParent> inventory = new Dictionary<string, WeaponParent>();

    string gunKey;
    bool hasGuns = false;

    public PlayerArgs pArgs;
    public WeaponParent heldGun;
    public int drawTime = 15;

    bool secondaryReleased = false;

    void OnEnable(){
        GameEvents.current.OnGetWeapon += GetWeapon;

        foreach (Animator a in viewmodelObject.GetComponentsInChildren<Animator>()){
            viewmodels.Add(a.gameObject.name, a);
            viewmodels[a.gameObject.name].gameObject.SetActive(false);
        }
        clocks.Add("draw", 0);
    }

    void OnDestroy(){
        GameEvents.current.OnGetWeapon -= GetWeapon;
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

        WeaponParent gunComponent = (WeaponParent)gameObject.AddComponent(Type.GetType(gun));
        gunComponent.InjectDependency(this);
        inventory.Add(gunKey, gunComponent);
    }

    void FixedUpdate(){
        playerHandler.WeaponUpdate(this);
    }

    WeaponParent weaponTest;
    void Update(){
        pArgs = playerHandler.playerArgs;

        if (!hasGuns){
            return;
        }

        if (InputManager.current.weapon0 && inventory.TryGetValue("Stakegun", out weaponTest) && gunKey != "Stakegun"){
            gunKey = "Stakegun";
            foreach (Animator a in viewmodels.Values){
                a.gameObject.SetActive(false);
            }
            foreach (WeaponParent w in inventory.Values){
                w.reloading = false;
            }

            StopCoroutine(Clock("draw", drawTime));
            StartCoroutine(Clock("draw", drawTime));
            viewmodels[gunKey].gameObject.SetActive(true);
        }

        if (InputManager.current.weapon1 && inventory.TryGetValue("Shotgun", out weaponTest ) && gunKey != "Shotgun"){
            gunKey = "Shotgun";
            foreach (Animator a in viewmodels.Values){
                a.gameObject.SetActive(false);
            }
            foreach (WeaponParent w in inventory.Values){
                w.reloading = false;
            }

            StopCoroutine(Clock("draw", drawTime));
            StartCoroutine(Clock("draw", drawTime));
            viewmodels[gunKey].gameObject.SetActive(true);
        }

        if (InputManager.current.weapon2 && inventory.TryGetValue("Revolver", out weaponTest) && gunKey != "Revolver"){
            gunKey = "Revolver";
            foreach (Animator a in viewmodels.Values){
                a.gameObject.SetActive(false);
            }
            foreach (WeaponParent w in inventory.Values){
                w.reloading = false;
            }
            
            StopCoroutine(Clock("draw", drawTime));
            StartCoroutine(Clock("draw", drawTime));
        }

        if (inventory.TryGetValue(gunKey, out heldGun)){
            heldGun = inventory[gunKey];
        }

        if (clocks["draw"] != 0){
            //while drawing weapon, no actions should be taken
            return;
        }

        if (InputManager.current.attacking && heldGun != null){
            if (heldGun.gunState != 0){
                // recovering or reloading
                return;
            }
            if (heldGun.loadedRounds == 0){
                // empty clip
                return;
            }
            Debug.Log("Primary Fire");
            heldGun.PrimaryFire(pArgs);
        }

        if (InputManager.current.attack2ing && heldGun != null){
            if (heldGun.gunState != 0){
                // recovering or reloading
                return;
            }
            if (heldGun.loadedRounds == 0){
                // empty clip
                return;
            }
            secondaryReleased = false;
            heldGun.SecondaryFire(pArgs);
        }

        if (InputManager.current.reload && heldGun != null){
            if (heldGun.gunState != 0){
                // recovering or reloading
                return;
            }
            heldGun.Reload();
        }

        if (!InputManager.current.attack2ing && heldGun != null && !secondaryReleased){
            if (heldGun.gunState != 0){
                // recovering or reloading
                return;
            }

            heldGun.SecondaryRelease(pArgs);
            secondaryReleased = true;
        }
    }

    public IEnumerator Clock(string id, int interval){
        int temp;
        //adds clock if it doesn't exist already
        if(!clocks.TryGetValue(id, out temp)){
            clocks.Add(id, 0);
        }

        int i = 0;
        while (i <= interval){
            i++;
            clocks[id] = i;

            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        i = 0;
        clocks[id] = 0;
    }
}
