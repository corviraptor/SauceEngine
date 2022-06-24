using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{      
    public PlayerHandler playerHandler;
    public GameObject viewmodelObject;

    public Dictionary<string, Animator> viewmodels = new Dictionary<string, Animator>();
    Dictionary<string, WeaponParent> inventory = new Dictionary<string, WeaponParent>();

    string gunKey;
    bool hasGuns = false;

    public PlayerArgs pArgs;

    void OnEnable(){
        GameEvents.current.OnGetWeapon += GetWeapon;

        foreach (Animator a in viewmodelObject.GetComponentsInChildren<Animator>()){
            viewmodels.Add(a.gameObject.name, a);
            viewmodels[a.gameObject.name].gameObject.SetActive(false);
        }
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
        viewmodels[gunKey].gameObject.SetActive(true);

        hasGuns = true;

        WeaponParent gunComponent = (WeaponParent)gameObject.AddComponent(Type.GetType(gun));
        gunComponent.InjectDependency(this);
        inventory.Add(gunKey, gunComponent);
    }

    WeaponParent heldGun;
    WeaponParent weaponTest;
    void Update(){
        pArgs = playerHandler.playerArgs;

        if (!hasGuns){
            return;
        }

        if(InputManager.current.weapon0 && inventory.TryGetValue("Stakegun", out weaponTest)){
            gunKey = "Stakegun";
            foreach (Animator a in viewmodels.Values){
                a.gameObject.SetActive(false);
            }
            viewmodels[gunKey].gameObject.SetActive(true);
        }

        if(InputManager.current.weapon1 && inventory.TryGetValue("Shotgun", out weaponTest)){
            gunKey = "Shotgun";
            foreach (Animator a in viewmodels.Values){
                a.gameObject.SetActive(false);
            }
            viewmodels[gunKey].gameObject.SetActive(true);
        }

        if(InputManager.current.weapon2 && inventory.TryGetValue("Revolver", out weaponTest)){
            gunKey = "Revolver";
            foreach (Animator a in viewmodels.Values){
                a.gameObject.SetActive(false);
            }
        }

        if(inventory.TryGetValue(gunKey, out heldGun)){
            heldGun = inventory[gunKey];
        }

        if (InputManager.current.attack && heldGun != null){
            heldGun.PrimaryFire(pArgs);
        }

        if (InputManager.current.attack2 && heldGun != null){
            heldGun.SecondaryFire(pArgs);
        }
    }
}
