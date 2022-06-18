using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : MonoBehaviour
{
    void OnEnable(){
        Debug.Log("WALKING ENABLED");
        PlayerMovementEvents.current.OnWalk += Walk;
    }

    void OnDestroy(){
        PlayerMovementEvents.current.OnWalk -= Walk;
    }

    void Walk(object sender, PlayerSettings player, Vector3 velocity, Vector3 accelXZ, RaycastHit hit, float walkSpeedAdj){
        //no friction after the first frame on the ground to let walks through queued jumps
        if (PlayerMovementEvents.current.clocks["frictionTimer"] >= 1){
            return;
        }
        if (PlayerMovementEvents.current.clocks["frictionTimer"] >= player.frictionForgiveness && new Vector3(velocity.x, 0, velocity.z).magnitude > player.overcomeThreshold){
            return;
        }
        Vector3 localWalkDirection = Vector3.ProjectOnPlane(accelXZ, hit.normal).normalized;
        Vector3 localWalkVector = localWalkDirection * accelXZ.magnitude * walkSpeedAdj;
        PlayerMovementEvents.current.velocity = localWalkVector;
    }
}
