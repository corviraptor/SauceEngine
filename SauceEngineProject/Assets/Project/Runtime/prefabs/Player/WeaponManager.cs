using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{   
    bool hasGuns = false;
    Dictionary<string, WeaponParent> inventory = new Dictionary<string, WeaponParent>();
    string gunKey;
    public PlayerArgs pArgs;

    void Start(){
        GameEvents.current.OnGetWeapon += GetWeapon;
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
        hasGuns = true;

        WeaponParent gunComponent = (WeaponParent)gameObject.AddComponent(Type.GetType(gun));
        inventory.Add(gunKey, gunComponent);
    }

    WeaponParent heldGun;
    WeaponParent weaponTest;
    void Update(){
        pArgs = PlayerHandler.current.playerArgs;

        if (!hasGuns){
            return;
        }

        if(InputManager.current.weapon0 && inventory.TryGetValue("Stakegun", out weaponTest)){
            gunKey = "Stakegun";
        }

        if(InputManager.current.weapon1 && inventory.TryGetValue("Shotgun", out weaponTest)){
            gunKey = "Shotgun";
        }

        if(InputManager.current.weapon2 && inventory.TryGetValue("Revolver", out weaponTest)){
            gunKey = "Revolver";
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
