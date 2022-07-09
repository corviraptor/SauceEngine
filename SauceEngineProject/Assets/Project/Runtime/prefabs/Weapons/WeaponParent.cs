using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponParent : MonoBehaviour
{   
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public PlayerWeapons pw;

    public Animator viewmodel;  

    public int chamberTime;
    public int loadTime;

    public bool fixedMag;
    public bool chambered;
    public int magSize;
    public int loadedRounds;
    public int roundsToLoad;
    public int reloadStage;

    protected WeaponParent(){}

    public abstract void InjectDependency(PlayerWeapons pw);

    public abstract void PlayReload();

    public abstract void PrimaryFire(PlayerArgs playerArgs);

    public abstract void PrimaryRelease(PlayerArgs playerArgs);

    public abstract void SecondaryFire(PlayerArgs playerArgs);

    public abstract void SecondaryRelease(PlayerArgs playerArgs);

    public abstract void WeaponSpell(PlayerArgs playerArgs);
}
