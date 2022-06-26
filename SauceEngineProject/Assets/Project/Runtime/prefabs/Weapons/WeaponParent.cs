using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponParent : MonoBehaviour
{   
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public WeaponManager weaponManager;

    public Animator viewmodel;  

    public bool reloading = false;
    public int gunState = 0;
    public int recoveryTime = 30;
    public int reloadTime = 30;
    public int magazineSize = 5;
    public int loadedRounds = 5;

    protected WeaponParent(){}

    public abstract void InjectDependency(WeaponManager wm);

    public abstract void PrimaryFire(PlayerArgs playerArgs);

    public abstract void SecondaryFire(PlayerArgs playerArgs);

    public abstract void SecondaryRelease(PlayerArgs playerArgs);

    public abstract void Reload();

    public abstract void WeaponSpell(PlayerArgs playerArgs);

    public virtual void Cycle(string name, int interval){
        if (gunState == 1){
            StopCoroutine(Clock(name, interval));
            StartCoroutine(Clock(name, interval));
        }
        if (gunState == 2){
            StopCoroutine(Clock(name, interval));
            StartCoroutine(Clock(name, interval));
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
        clocks[id] = i;
        gunState = 0;
    }
}
