using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    public PlayerMovement playerMovement;
    CharacterController controller;

    void OnEnable(){
        playerMovement.OnCrouch += Crouch;
        controller = playerMovement.cc;
    }

    void OnDestroy(){
        playerMovement.OnCrouch -= Crouch;
    }

    void Crouch(object sender, PlayerSettings player, int crouchState, bool isOnGround, RaycastHit hit){
        // 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (crouchState == 0 && isOnGround && playerMovement.clocks["jumpBuffer"] == 0 && hit.point.y <= playerMovement.center.y - (controller.height/2)){
            //forces player up to avoid collider expanding into the ground. doesnt activate when player jump is being processed
            controller.enabled = false;
            playerMovement.transform.position = new Vector3(playerMovement.transform.position.x, hit.point.y - controller.center.y + controller.height/2 + 0.1F, playerMovement.transform.position.z);
            controller.enabled = true;

            return;
        }
        // is crouched if it gets past this
        if (crouchState == 2 && isOnGround && playerMovement.clocks["jumpBuffer"] == 0 && hit.point.y <= playerMovement.center.y - (controller.height/2)){
            //when the player is on the ground, this moves them down when crouching to keep their feet at the same level, essentially meaning they just duck instead of having to fall after picking up their feet
            controller.enabled = false;
            playerMovement.transform.position = new Vector3(playerMovement.transform.position.x, hit.point.y - controller.center.y + controller.height/2 + 0.05F, playerMovement.transform.position.z);
            controller.enabled = true;

            return;
        }
    }
}