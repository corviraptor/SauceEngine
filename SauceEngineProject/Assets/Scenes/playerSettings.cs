using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "playerSettings", menuName = "SauceEngineProject/playerSettings", order = 0)]
public class playerSettings : ScriptableObject{
    public float sens = 0.7f;
    public float moveSpeed = 40F;
    public float vLimit = 20F;
    public float walkLimit = 10F;
    public float walkSpeed = 0.5F;
    public float height = 2F;
    public float jumpForce = 100F;
    public float gravity = 9.81F;
    public float fallMultiplier = 1.2F;
    public float groundPull = 6F;
    public float terminalVelocity = 56F;
    public float gThreshold = 0.1F;
    public float maxSlope = 0.4F;
    [Range(0f, 1f)]
    public float frictionFactor = 0.98F; //should always be between 0 and 1
    //forgiveness thresholds, in ticks
    public int frictionForgiveness = 10;
    public float overcomeThreshold = 5F;
    public float frictionCutoff = 5;
    public int jumpForgiveness = 5;
    public int jumpCooldown = 15;
    public int coyoteTime = 10;
}
    