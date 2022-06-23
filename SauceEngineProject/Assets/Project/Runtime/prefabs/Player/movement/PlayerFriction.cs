using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFriction : MonoBehaviour
{
    public PlayerMovement playerMovement;
    void OnEnable(){
        playerMovement.OnFriction += Friction;
    }

    void OnDestroy(){
        playerMovement.OnFriction -= Friction;
    }

    void Friction(object sender, PlayerSettings player, Vector3 velocity){
        //Debug.Log(playerMovement.clocks["frictionTimer"]);
        //no friction after the first frame on the ground to let bhops through queued jumps
        if (playerMovement.clocks["frictionTimer"] >= 1){
            return;
        }
        //inequalities here are flipped from what they are in walk since this isn't a guard statement
        if (playerMovement.clocks["frictionTimer"] <= player.frictionForgiveness || new Vector3(velocity.x, 0, velocity.z).magnitude > player.overcomeThreshold){
            velocity -= new Vector3(velocity.x, 0, velocity.z) * player.frictionFactor * Time.deltaTime;
            playerMovement.velocity = velocity;
        }
    }
}