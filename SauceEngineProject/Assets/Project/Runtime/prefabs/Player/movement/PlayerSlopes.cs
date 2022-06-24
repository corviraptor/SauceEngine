using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Slope(object sender){
        //pm.slopeState: 0 = not on slope, 1 = slipping, 2 = surfing
        if (pm.slopeState == 2){
            //surf lift - lets pm.player ride at an angle that keeps them from losing speed
            Vector3 surfaceX = Vector3.Cross(transform.up, pm.hit.normal);
            Vector3 surfaceZ = Vector3.Cross(pm.hit.normal, surfaceX);
            pm.velocity += surfaceZ * pm.player.surfLift * Time.deltaTime;
        }
        else if (pm.slopeState == 1){
            Vector3 velocityXZ = pm.velocity.KillY();
            float slip = velocityXZ.magnitude * Time.deltaTime * 20;
            Vector3 slipVector = Vector3.Lerp(-transform.up, pm.hit.normal, 0.4F) * slip;
            pm.velocity += slipVector;

        }
    }
}
