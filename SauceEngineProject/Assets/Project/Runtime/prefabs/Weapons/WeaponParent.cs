using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponParent : MonoBehaviour
{   
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public PlayerWeapons playerWeapons;

    public Animator viewmodel;  

    public bool internalMagazine = false;

    public int recoveryTime = 30;
    public int reloadTime = 30;
    public int magazineSize = 5;
    public int loadedRounds = 5;

    protected WeaponParent(){}

    public abstract void InjectDependency(PlayerWeapons pw);

    public abstract void PrimaryFire(PlayerArgs playerArgs);

    public abstract void PrimaryRelease(PlayerArgs playerArgs);

    public abstract void SecondaryFire(PlayerArgs playerArgs);

    public abstract void SecondaryRelease(PlayerArgs playerArgs);

    public abstract void WeaponSpell(PlayerArgs playerArgs);
}
