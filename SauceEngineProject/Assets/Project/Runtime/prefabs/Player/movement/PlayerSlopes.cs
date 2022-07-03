using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Freya;

public class PlayerSlopes : MonoBehaviour, IAttachable
{
    PlayerMovement pm;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        pm.OnSlope += Slope;
    }

    void OnDestroy(){
        pm.OnSlope -= Slope;
    }

    void Slope(){
        Vector3 surfaceX = Vector3.Cross(transform.up, pm.margs.hit.normal);
        Vector3 surfaceZ = Vector3.Cross(pm.margs.hit.normal, surfaceX);

        //pm.slopeState: 0 = not on slope, 1 = slipping, 2 = surfing
        if (pm.margs.slopeState == 1){
            Vector3 velocityXZ = pm.velocity.KillY();
            float slip = velocityXZ.magnitude * Time.deltaTime;
            Vector3 slipVector = surfaceZ * slip;
            pm.velocity += slipVector;
        }
    }
}
