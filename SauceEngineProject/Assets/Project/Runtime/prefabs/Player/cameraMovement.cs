using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Freya;

public class CameraMovement : MonoBehaviour
{
    public PlayerHandler playerHandler;
    public PlayerSettings player;
    float mousePitch;
    Transform playerTransform;
    float playerHeight;

    void OnEnable(){
        playerHandler.OnPlayerPositionUpdate += PosUpdate;
        mousePitch = 0;
    }

    // should be called *after* LateUpdate
    private void PosUpdate(object sender, PlayerArgs playerArgs){
        playerTransform = playerArgs.transform;
        playerHeight = playerArgs.controller.height;
        
        float mouseY = (-InputManager.current.lookVector.y);
        mousePitch += mouseY;
        mousePitch = Mathfs.Clamp(mousePitch, -90, 90);
        transform.eulerAngles = new Vector3(mousePitch, playerTransform.eulerAngles.y, playerTransform.eulerAngles.z);
        transform.position = playerTransform.position + (playerArgs.controller.center) + Vector3.up * (playerArgs.controller.height / 2 - player.height / 4);

        playerHandler.playerArgs.cameraTransform = transform;
    }

    void OnDestroy() {
        playerHandler.OnPlayerPositionUpdate -= PosUpdate;
    }
}