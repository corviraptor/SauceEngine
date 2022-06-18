using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    public CharacterController controller;

    void OnEnable(){
        Debug.Log("CROUCHING ENABLED");
        PlayerMovementEvents.current.OnCrouch += Crouch;
        controller = PlayerMovementEvents.current.cc;
    }

    void OnDestroy(){
        PlayerMovementEvents.current.OnCrouch -= Crouch;
    }

    void Crouch(object sender, PlayerSettings player, int crouchState, bool isOnGround, RaycastHit hit){
        // 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (crouchState == 0 && isOnGround && PlayerMovementEvents.current.clocks["jumpBuffer"] == 0){
            //forces player up to avoid collider expanding into the ground. doesnt activate when player jump is being processed
            controller.enabled = false;
            PlayerMovementEvents.current.transform.position = new Vector3(PlayerMovementEvents.current.transform.position.x, hit.point.y - controller.center.y + controller.height/2 + 0.1F, PlayerMovementEvents.current.transform.position.z);
            controller.enabled = true;
            return;
        }
        // is crouched if it gets past this
        if (isOnGround && PlayerMovementEvents.current.clocks["jumpBuffer"] == 0){
            //when the player is on the ground, this moves them down when crouching to keep their feet at the same level, essentially meaning they just duck instead of having to fall after picking up their feet
            controller.enabled = false;
            PlayerMovementEvents.current.transform.position = new Vector3(PlayerMovementEvents.current.transform.position.x, hit.point.y - controller.center.y + controller.height/2 + 0.05F, PlayerMovementEvents.current.transform.position.z);
            controller.enabled = true;
        }
    }
}
