using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    public playerSettings player;
    float mousePitch = 0;
    Transform playerTransform;
    float playerHeight;

    void Start(){
        GameEvents.current.onPlayerPositionUpdate += posUpdate;
    }

    // should be called *after* LateUpdate
    private void posUpdate(object sender, Transform pTransform, float height, Vector3 center){
        playerTransform = pTransform;
        playerHeight = height;


        float mouseY = (-player.sens * Input.GetAxis("Mouse Y"));
        mousePitch += mouseY;
        mousePitch = Mathf.Clamp(mousePitch, -90, 90);
        transform.eulerAngles = new Vector3(mousePitch, playerTransform.eulerAngles.y, playerTransform.eulerAngles.z);
        //transform position plus the playercontroller's "center" vector = player's true center in worldspace
        transform.position = playerTransform.position + center + (Vector3.up * playerHeight / 4.1F);
    }

    void OnDestroy() {
        GameEvents.current.onPlayerPositionUpdate -= posUpdate;
    }
}