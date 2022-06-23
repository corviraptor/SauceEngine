using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public PlayerMovement playerMovement;
    void OnEnable(){
        playerMovement.OnJump += Jump;
    }

    void OnDestroy(){
        playerMovement.OnJump -= Jump;
    }

    //called on spacebar press & during timer 
    void Jump(object sender, PlayerSettings player, Vector3 velocity, bool isOnGround, RaycastHit hit){

        if (isOnGround){

            velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
            playerMovement.velocity = velocity;

            playerMovement.StartJumpCooldown();
            GameEvents.current.SoundCommand("Jump", "Play", 0);

            return;
        }
        if (playerMovement.clocks["coyoteTime"] <= player.coyoteTime && playerMovement.clocks["coyoteTime"] != 0){

            velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
            playerMovement.velocity = velocity;

            playerMovement.StartJumpCooldown();
            GameEvents.current.SoundCommand("Jump", "Play", 0);

            return;
        }
    }
}
