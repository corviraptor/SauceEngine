using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour, IAttachable
{
    PlayerMovement pm;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        pm.OnJump += Jump;
    }

    void OnDestroy(){
        pm.OnJump -= Jump;
    }

    //called on spacebar press & during timer 
    void Jump(){
        if (pm.margs.isOnGround){

            pm.velocity = new Vector3(pm.velocity.x, pm.player.jumpForce, pm.velocity.z);

            pm.StartJumpCooldown();

            return;
        }
        if (pm.clocks["coyoteTime"] <= pm.player.coyoteTime && pm.clocks["coyoteTime"] != 0){

            pm.velocity = new Vector3(pm.velocity.x, pm.player.jumpForce, pm.velocity.z);

            pm.StartJumpCooldown();

            return;
        }
    }
}
