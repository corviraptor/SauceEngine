using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFriction : MonoBehaviour, IAttachable
{
    PlayerMovement pm;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        pm.OnFriction += Friction;
    }

    void OnDestroy(){
        pm.OnFriction -= Friction;
    }

    void Friction(){
        //inequalities here are flipped from what they are in walk since this isn't a guard statement
        if (!pm.frictionForgiven){
            pm.velocity -= pm.velocity.KillY() * pm.player.frictionFactor * Time.deltaTime;
        }
    }
}