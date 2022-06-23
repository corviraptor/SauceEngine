using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFriction : MonoBehaviour
{
    void OnEnable(){
        PlayerMovementEvents.current.OnFriction += Friction;
    }

    void OnDestroy(){
        PlayerMovementEvents.current.OnFriction -= Friction;
    }

    void Friction(object sender, PlayerSettings player, Vector3 velocity){
        //Debug.Log(PlayerMovementEvents.current.clocks["frictionTimer"]);
        //no friction after the first frame on the ground to let bhops through queued jumps
        if (PlayerMovementEvents.current.clocks["frictionTimer"] >= 1){
            return;
        }
        //inequalities here are flipped from what they are in walk since this isn't a guard statement
        if (PlayerMovementEvents.current.clocks["frictionTimer"] <= player.frictionForgiveness || new Vector3(velocity.x, 0, velocity.z).magnitude > player.overcomeThreshold){
            velocity -= new Vector3(velocity.x, 0, velocity.z) * player.frictionFactor * Time.deltaTime;
            PlayerMovementEvents.current.velocity = velocity;
        }
    }
}