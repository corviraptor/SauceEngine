using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArgs
{   
    public Vector3 velocity;
    public Vector3 localVelocity;
    public float temperature;
    public Transform transform;
    public CharacterController controller;
    public Transform cameraTransform;

    public PlayerArgs(Vector3 v, Vector3 lv, float h, Transform tf, CharacterController cc, Transform ct){
        velocity = v;
        localVelocity = lv;
        temperature = h;
        transform = tf;
        controller = cc;
        cameraTransform = ct;
    }
}
