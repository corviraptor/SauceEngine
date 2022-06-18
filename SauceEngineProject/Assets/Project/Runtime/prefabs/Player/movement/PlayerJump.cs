using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    void OnEnable(){
        Debug.Log("JUMPING ENABLED");
        PlayerMovementEvents.current.OnJump += Jump;
    }

    void OnDestroy(){
        PlayerMovementEvents.current.OnJump -= Jump;
    }

    //called on spacebar press & during timer 
    void Jump(object sender, PlayerSettings player, Vector3 velocity, bool isOnGround, RaycastHit hit){

        if (isOnGround){

            velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
            PlayerMovementEvents.current.velocity = velocity;

            PlayerMovementEvents.current.StartJumpCooldown();
            GameEvents.current.playerJump(this);

            return;
        }
        if (PlayerMovementEvents.current.clocks["coyoteTime"] <= player.coyoteTime && PlayerMovementEvents.current.clocks["coyoteTime"] != 0){

            velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
            PlayerMovementEvents.current.velocity = velocity;

            PlayerMovementEvents.current.StartJumpCooldown();
            GameEvents.current.playerJump(this);

            return;
        }
    }
}
