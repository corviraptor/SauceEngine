using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
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
            PlayerHandler.current.OnPlayerPositionUpdate += PosUpdate;
            mousePitch = 0;
            initialized = true;
        }
    }

    // should be called *after* LateUpdate
    private void PosUpdate(object sender, Transform pTransform, float height, Vector3 center){
        playerTransform = pTransform;
        playerHeight = height;

        float mouseY = (-InputManager.current.lookVector.y);
        mousePitch += mouseY;
        mousePitch = Mathf.Clamp(mousePitch, -90, 90);
        transform.eulerAngles = new Vector3(mousePitch, playerTransform.eulerAngles.y, playerTransform.eulerAngles.z);
        //transform position plus the playercontroller's "center" vector = player's true center in worldspace
        transform.position = playerTransform.position + Vector3.up * height / 4;

        PlayerHandler.current.playerArgs.cameraTransform = transform;
    }

    void OnDestroy() {
        PlayerHandler.current.OnPlayerPositionUpdate -= PosUpdate;
    }
}