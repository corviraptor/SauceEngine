using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    public controlCfg cfg;
    float mousePitch = 0;
    private GameObject Player;

    void Start()
    {
        Player = GameObject.Find("Player");
    }

    void LateUpdate()
    {
        // aim pitch
        float mouseY = (-cfg.sens * Input.GetAxis("Mouse Y"));
        mousePitch += mouseY;
        mousePitch = Mathf.Clamp(mousePitch, -90, 90);
        transform.eulerAngles = new Vector3(mousePitch, transform.eulerAngles.y, transform.eulerAngles.z);
        //Vector3 camParPosition = camPar.transform.position;
        //Vector3 camParRotation = camPar.transform.rotation;
        //transform.position = camParPosition;
        //transform.rotation = camParRotation;
    }
}