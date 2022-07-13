using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    [SerializeField] private GameObject viewmodelObject;
    [SerializeField] PlayerHandler playerHandler;
    public IShootable gun;
    FirstPersonActions.PlayerActions input => InputManager.current.input;
    PlayerArgs pArgs;

    Dictionary<string, IShootable> inventory = new Dictionary<string, IShootable>();
    Dictionary<string, Animator> vm = new Dictionary<string, Animator>(); 
    public Dictionary<string, Animator> viewmodels {get {return vm;}}


    bool hasAGun = false;
    bool primaryReleased = true;
    bool secondaryReleased = true;
    string gunKey;
    int drawTime = 25;


    void OnEnable(){
        GameEvents.current.OnGetWeapon += GetWeapon;
        InputManager.current.OnPressButtons += OnPressButtons;

        foreach (Animator a in viewmodelObject.GetComponentsInChildren<Animator>()){
            vm.Add(a.gameObject.name, a);
            vm[a.gameObject.name].gameObject.SetActive(false);
        }
    }

    void OnDestroy(){
        GameEvents.current.OnGetWeapon -= GetWeapon;
        InputManager.current.OnPressButtons -= OnPressButtons;
    }

    void OnPressButtons(Dictionary<string, bool> buttons){
        // none of this should run if the player doesn't have a gun
        if (hasAGun == false){ return; }

        /* these are hard coded since i want each binding to correspond to a specific gun no matter what, i'll probably turn this
        into something using an array instead tho to make it easier to modify and extend */
        if (buttons["weapon0"]){ SwapGun("Stakegun"); }
        if (buttons["weapon1"]){ SwapGun("Shotgun"); }
    }

    void SwapGun(string g){
        IShootable weaponTest;
        if (inventory.TryGetValue(g, out weaponTest ) && gunKey != g){ 
            gunKey = g; 
            StopAllCoroutines();
            StartCoroutine(Draw());
        }
    }

    void GetWeapon(object sender, string g){
        gunKey = g;
        if (gunKey == null){ return; }
        if (gameObject.GetComponent(Type.GetType(gunKey)) != null){ return; }

        IShootable gunComponent = (IShootable)gameObject.AddComponent(Type.GetType(gunKey));
        gunComponent.InjectDependency(this);
        inventory.Add(gunKey, gunComponent);

        StopAllCoroutines();
        StartCoroutine(Draw());
        hasAGun = true;
    }

    IEnumerator Draw(){

        foreach (Animator a in vm.Values){ a.gameObject.SetActive(false); }

        if (gun != null){
            MonoBehaviour gunBehaviour = (MonoBehaviour)gun;
            gunBehaviour.StopAllCoroutines();
        }

        gun = inventory[gunKey];
        gun.Draw(drawTime);
        vm[gunKey].gameObject.SetActive(true);

        // reset fuckin everything
        primaryReleased = false;
        secondaryReleased = false;
        gun.chambered = false;


        yield return new WaitForSeconds(drawTime / 60F);

        gun.chambered = true;
        StopAllCoroutines();
    }
    
    void Update(){
        pArgs = playerHandler.playerArgs;

        if (hasAGun == false){ return; }

        playerHandler.WeaponUpdate(this);

        if (inventory.TryGetValue(gunKey, out gun)){ gun = inventory[gunKey]; }

        EvaluateGunInputs();
    }
    
    void EvaluateGunInputs(){
        // function won't be called if we are in the middle of drawing a gun or there is no held gun
        if (input.PrimaryFire.ReadValue<float>() != 0){
            primaryReleased = false;
            gun.PrimaryFire(pArgs);
        }

        if (input.SecondaryFire.ReadValue<float>() != 0){
            secondaryReleased = false;
            gun.SecondaryFire(pArgs);
        }

        if (input.PrimaryFire.ReadValue<float>() == 0 && !primaryReleased){
            primaryReleased = true;
            gun.PrimaryRelease();
        }

        if (input.SecondaryFire.ReadValue<float>() == 0 && !secondaryReleased){
            secondaryReleased = true;
            gun.SecondaryRelease();
        }

        if (input.Reload.ReadValue<float>() != 0){
            gun.Reload();
        }
    }  

    public void PlayViewmodelAnimation (string name){
        vm[gunKey].SetTrigger(name);
    }
}