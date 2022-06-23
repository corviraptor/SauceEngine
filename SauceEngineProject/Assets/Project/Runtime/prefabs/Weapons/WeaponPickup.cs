using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{      
    public string gun = "Stakegun";

    void OnTriggerEnter(Collider other) {
        if(other.tag != "Player"){
            return;
        }
        GameEvents.current.GetWeapon(this, gun);
    }
}
