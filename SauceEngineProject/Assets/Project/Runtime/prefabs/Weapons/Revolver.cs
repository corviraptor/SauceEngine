using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : WeaponParent
{
    public override void InjectDependency(WeaponManager wm){
        weaponManager = wm;
    }

    void Update(){
        if (loadedRounds == 0 && gunState == 0){
            Reload();
        }
        if (gunState == 2){
            reloading = true;
        }
        if (gunState == 0 && reloading == true){
            //only add rounds to magazine when finished reloading
            loadedRounds = magazineSize;
        }
    }

    public override void PrimaryFire(PlayerArgs playerArgs){
    }
    
    public override void SecondaryFire(PlayerArgs playerArgs){
    }

    public override void SecondaryRelease(PlayerArgs playerArgs){}

    public override void Reload(){}

    public override void WeaponSpell(PlayerArgs playerArgs){}

    public override void Cycle(string name, int interval){
        base.Cycle("Revolver", interval); 
    }
}
