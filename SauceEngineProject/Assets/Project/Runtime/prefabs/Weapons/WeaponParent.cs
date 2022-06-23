using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponParent : MonoBehaviour
{   
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public int gunState = 0;
    public int recoveryTime = 10;
    public int actionTime = 10;
    public int reloadTime = 10;
    public int magazineSize = 5;
    public int loadedRounds = 5;

    protected WeaponParent(){}
    
    public virtual void Inputs(PlayerArgs playerArgs){
        if(InputManager.current.attack){ PrimaryFire(playerArgs); }
        if(InputManager.current.attack2){ SecondaryFire(playerArgs); }
    }

    public abstract void PrimaryFire(PlayerArgs playerArgs);

    public abstract void SecondaryFire(PlayerArgs playerArgs);

    public abstract void WeaponSpell(PlayerArgs playerArgs);

    public virtual void Cycle(string name){
        if (gunState == 1){
            StartCoroutine(Clock(name, recoveryTime));
            Debug.Log("Recovering");
        }
        if (gunState == 2){
            StartCoroutine(Clock(name, actionTime));
            Debug.Log("Cycling the Action");
        }
        if (gunState == 3){
            StartCoroutine(Clock(name, reloadTime));
            Debug.Log("Reloading");
        }
        if (gunState > 3){
            gunState = 0;
            return;
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
        gunState++;
        this.Cycle(id);
    }
}
