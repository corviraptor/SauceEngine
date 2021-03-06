using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirControl : MonoBehaviour, IAttachable
{
    PlayerMovement pm;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        pm.OnAirControl += AirControl;
    }

    void OnDestroy(){
        pm.OnAirControl -= AirControl;
    }
    // Update is called once per frame
    void AirControl(){
        // speed limit
        Vector3 velocityXZ = pm.velocity.KillY();
        float AVproj = Vector3.Dot(velocityXZ, pm.margs.wishDir * pm.player.airAccel);

        if (AVproj < pm.player.vLimit - (pm.margs.wishDir.magnitude * Time.deltaTime))
        { 
            velocityXZ = velocityXZ + (pm.margs.wishDir * pm.player.airAccel * Time.deltaTime);
            pm.velocity = new Vector3(velocityXZ.x, pm.velocity.y, velocityXZ.z);
        }
    }
}
