using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour, IAttachable
{
    PlayerMovement pm;
    CharacterController controller;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        controller = pm.cc;
        pm.OnCrouch += Crouch;
    }

    void OnDestroy(){
        pm.OnCrouch -= Crouch;
    }

    void Crouch(object sender){
        // 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (pm.crouchState == 0 && pm.isOnGround && pm.clocks["jumpBuffer"] == 0 && pm.hit.point.y <= pm.center.y - (controller.height/2)){
            //forces player up to avoid collider expanding into the ground. doesnt activate when player jump is being processed
            controller.enabled = false;
            pm.transform.position = new Vector3(pm.transform.position.x, pm.hit.point.y - controller.center.y + controller.height/2 + 0.1F, pm.transform.position.z);
            controller.enabled = true;

            return;
        }
        // is crouched if it gets past this
        if (pm.crouchState == 2 && pm.isOnGround && pm.clocks["jumpBuffer"] == 0 && pm.hit.point.y <= pm.center.y - (controller.height/2)){
            //when the player is on the ground, this moves them down when crouching to keep their feet at the same level, essentially meaning they just duck instead of having to fall after picking up their feet
            controller.enabled = false;
            pm.transform.position = new Vector3(pm.transform.position.x, pm.hit.point.y - controller.center.y + controller.height/2 + 0.05F, pm.transform.position.z);
            controller.enabled = true;

            return;
        }
    }
}