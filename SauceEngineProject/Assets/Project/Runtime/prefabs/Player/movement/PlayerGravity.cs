using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    void OnEnable(){
        PlayerMovementEvents.current.OnGravity += Gravity;
    }

    void OnDestroy(){
        PlayerMovementEvents.current.OnGravity -= Gravity;
    }

    void Gravity(object sender, PlayerSettings player, Vector3 velocity, RaycastHit hit){
        //no gravity added if player is falling faster than terminal velocity
        if (velocity.y <= player.terminalVelocity){
            Debug.Log("terminal velocity reached");
            return;
        }

        Vector3 gravityVector = new Vector3(0, player.gravity * Time.deltaTime, 0);
        
        //strong gravity
        if (velocity.y < 0 && hit.distance <= player.groundPull && Vector3.Dot(hit.normal, transform.up) > player.maxSlope && PlayerMovementEvents.current.clocks["postJumpGravity"] != 0){
            gravityVector = gravityVector * player.fallMultiplier;
        }

        velocity -= gravityVector;
        PlayerMovementEvents.current.velocity = velocity;
    }
}