using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : WeaponParent
{
    public override void InjectDependency(PlayerWeapons pw){
        playerWeapons = pw;
    }

    void Update(){
    }

    public override void PrimaryFire(PlayerArgs playerArgs){
    }

    public override void PrimaryRelease(PlayerArgs playerArgs){
    }
    
    public override void SecondaryFire(PlayerArgs playerArgs){
    }

    public override void SecondaryRelease(PlayerArgs playerArgs){
    }

    public override void WeaponSpell(PlayerArgs playerArgs){
    }
}
