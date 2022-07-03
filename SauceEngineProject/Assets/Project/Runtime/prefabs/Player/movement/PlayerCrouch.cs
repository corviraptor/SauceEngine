using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour, IAttachable
{
    PlayerMovement pm;
    CharacterController controller;
    bool crouched = false;

    public void InjectDependency(PlayerMovement playerMovement){
        pm = playerMovement;
        controller = pm.cc;
        pm.OnCrouch += Crouch;
    }

    void OnDestroy(){
        pm.OnCrouch -= Crouch;
    }

    void Crouch(){
        // 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (pm.margs.crouchState == 0){
            if (pm.margs.hit.point.y < (pm.pargs.center - Vector3.up * (controller.height / 2)).y){
                Uncrouch();
            }
            return;
        }
        // is crouched if it gets past this

        if (pm.margs.crouchState != 2){ return; }
        // is manually crouching if it gets past this

        if (crouched){ return; }
        // first frame of crouch input after this
        
        controller.center = Vector3.up * -0.5F;
        controller.height = pm.player.height / 2;
        pm.walkSpeedAdj = pm.player.walkSpeed / 2;

        crouched = true;
        
        if (!pm.margs.isOnGrounder || pm.clocks["jumpBuffer"] != 0){
            //when the player is off the ground, this moves them up to keep their eyes at the same level
            Vector3 newPosition = pm.transform.position + Vector3.up * controller.height;
            MoveTransform(newPosition);
        }
    }

    void Uncrouch(){
        if (crouched){
            Vector3 newPosition = pm.transform.position - Vector3.up * controller.height;
            MoveTransform(newPosition);

            controller.center = Vector3.zero;
            controller.height = pm.player.height;
            pm.walkSpeedAdj = pm.player.walkSpeed;
        
            crouched = false;
        }
    }

    void MoveTransform(Vector3 newPosition){
        controller.enabled = false;
        pm.pargs.transform.position = newPosition;
        controller.enabled = true;
    }
}