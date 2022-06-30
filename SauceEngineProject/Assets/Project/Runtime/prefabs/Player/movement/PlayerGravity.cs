using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour, IAttachable
{
    PlayerMovement pm;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        pm.OnGravity += Gravity;
    }

    void OnDestroy(){
        pm.OnGravity -= Gravity;
    }

    void Gravity(){
        //no gravity added if player is falling faster than terminal velocity
        if (pm.velocity.y <= pm.player.terminalVelocity){
            Debug.Log("terminal velocity reached");
            return;
        }

        Vector3 gravityVector = Vector3.up * pm.player.gravity * Time.deltaTime;

        pm.velocity -= gravityVector;
    }
}