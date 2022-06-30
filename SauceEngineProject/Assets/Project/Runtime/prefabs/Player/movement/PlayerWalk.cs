using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : MonoBehaviour, IAttachable
{
    PlayerMovement pm;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        pm.OnWalk += Walk;
    }

    void OnDestroy(){
        pm.OnWalk -= Walk;
    }

    void Walk(){
        if (pm.frictionForgiven){
            return;
        }
        Vector3 localWalkDirection = Vector3.ProjectOnPlane(pm.accelXZ, pm.hit.normal).normalized;
        Vector3 localWalkVector = localWalkDirection * pm.accelXZ.magnitude * pm.walkSpeedAdj;
        pm.velocity = pm.velocity - pm.player.walkAcceleration * pm.velocity * Time.deltaTime + pm.player.walkAcceleration * localWalkVector * Time.deltaTime;
    }
}
