using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlopes : MonoBehaviour
{
    void OnEnable(){
        Debug.Log("SLOPE BEHAVIOR ENABLED");
        PlayerMovementEvents.current.OnSlope += Slope;
    }

    void OnDestroy(){
        PlayerMovementEvents.current.OnSlope -= Slope;
    }

    void Slope(object sender, PlayerSettings player, Vector3 velocity, int slopeState, RaycastHit hit){
        //slopeState: 0 = not on slope, 1 = slipping, 2 = surfing
        if (slopeState == 2){
            //surf lift - lets player ride at an angle that keeps them from losing speed
            Vector3 surfaceX = Vector3.Cross(transform.up, hit.normal);
            Vector3 surfaceZ = Vector3.Cross(hit.normal, surfaceX);
            velocity += surfaceZ * player.surfLift * Time.deltaTime;
        }
        else if (slopeState == 1){
            Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
            float slip = velocityXZ.magnitude * Time.deltaTime * 20;
            Vector3 slipVector = Vector3.Lerp(-transform.up, hit.normal, 0.4F) * slip;
            velocity += slipVector;

        }
    }
}
