using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirControl : MonoBehaviour
{
    void OnEnable(){
        Debug.Log("AIR CONTROL ENABLED");
        PlayerMovementEvents.current.OnAirControl += AirControl;
    }

    void OnDestroy(){
        PlayerMovementEvents.current.OnAirControl -= AirControl;
    }

    // Update is called once per frame
    void AirControl(object sender, PlayerSettings player, Vector3 velocity, Vector3 accelXZ){
        // speed limit
        Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
        float AVproj = Vector3.Dot(velocityXZ, accelXZ * player.airAccel);

        if (AVproj < player.vLimit - (accelXZ.magnitude * Time.deltaTime))
        { 
            velocityXZ = velocityXZ + (accelXZ * player.airAccel * Time.deltaTime);
            PlayerMovementEvents.current.velocity = new Vector3(velocityXZ.x, velocity.y, velocityXZ.z);
        }
    }
}
