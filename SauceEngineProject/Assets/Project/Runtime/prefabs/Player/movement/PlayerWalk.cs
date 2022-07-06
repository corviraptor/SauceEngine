using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Freya;

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
        if (pm.margs.frictionForgiven){
            return;
        }
        Vector3 localWalkDirection = Vector3.ProjectOnPlane(pm.margs.wishDir, pm.margs.hit.normal).normalized;
        Vector3 localWalkVector = localWalkDirection * pm.margs.wishDir.magnitude * pm.walkSpeedAdj;
        if (pm.velocity.magnitude <= pm.walkSpeedAdj){
            pm.velocity += pm.player.walkAcceleration * localWalkVector * Time.deltaTime;
        }
        else{
            pm.velocity = (pm.velocity - pm.velocity * Time.deltaTime + pm.player.walkAcceleration * localWalkVector * Time.deltaTime).normalized * pm.walkSpeedAdj;
        }
    }
}
