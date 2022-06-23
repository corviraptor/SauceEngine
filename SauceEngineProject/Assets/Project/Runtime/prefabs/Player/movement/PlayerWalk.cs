using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : MonoBehaviour
{
    public PlayerMovement playerMovement;
    void OnEnable(){
        playerMovement.OnWalk += Walk;
    }

    void OnDestroy(){
        playerMovement.OnWalk -= Walk;
    }

    void Walk(object sender, PlayerSettings player, Vector3 velocity, Vector3 accelXZ, RaycastHit hit, float walkSpeedAdj){
        //no friction after the first frame on the ground to let walks through queued jumps
        if (playerMovement.clocks["frictionTimer"] >= 1){
            return;
        }
        if (playerMovement.clocks["frictionTimer"] >= player.frictionForgiveness && velocity.KillY().magnitude > player.overcomeThreshold){
            return;
        }
        Vector3 localWalkDirection = Vector3.ProjectOnPlane(accelXZ, hit.normal).normalized;
        Vector3 localWalkVector = localWalkDirection * accelXZ.magnitude * walkSpeedAdj;
        playerMovement.velocity = velocity - player.walkAcceleration * velocity * Time.deltaTime + player.walkAcceleration * localWalkVector * Time.deltaTime;
    }
}
