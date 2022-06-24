using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public PlayerHandler playerHandler;
    public PlayerSettings player;
    float mousePitch;
    Transform playerTransform;
    float playerHeight;

    bool initialized;

    void OnEnable(){
        initialized = false;
    }

    void Update(){
        if (!initialized){
            playerHandler.OnPlayerPositionUpdate += PosUpdate;
            mousePitch = 0;
            initialized = true;
        }
    }

    // should be called *after* LateUpdate
    private void PosUpdate(object sender, PlayerArgs playerArgs){
        playerTransform = playerArgs.transform;
        playerHeight = playerArgs.controller.height;

        float mouseY = (-InputManager.current.lookVector.y);
        mousePitch += mouseY;
        mousePitch = Mathf.Clamp(mousePitch, -90, 90);
        transform.eulerAngles = new Vector3(mousePitch, playerTransform.eulerAngles.y, playerTransform.eulerAngles.z);
        //transform position plus the playerArgs.center = player's true center in worldspace
        transform.position = playerTransform.position + Vector3.up * playerArgs.controller.height / 4;

        playerHandler.playerArgs.cameraTransform = transform;
    }

    void OnDestroy() {
        playerHandler.OnPlayerPositionUpdate -= PosUpdate;
    }
}