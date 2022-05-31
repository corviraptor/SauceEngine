using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings", order = 0)]
public class PlayerSettings : ScriptableObject{
    public float sens = 0.7f;
    public float airAccel = 40F;
    public float vLimit = 20F;
    public float walkSpeed = 20F;
    public float height = 2F;
    public float jumpForce = 100F;
    public float gravity = 9.81F;
    public float fallMultiplier = 1.2F;
    public float groundPull = 6F;
    public float terminalVelocity = 56F;
    public float gThreshold = 0.1F;
    public float gMagnetism = 3F;
    public float gMagThreshold = 0.5F;
    public float maxSlope = 0.4F;
    public float surfLift = 10F;
    [Range(0f, 1f)]
    public float frictionFactor = 0.98F; //should always be between 0 and 1
    //forgiveness thresholds, in ticks
    public int frictionForgiveness = 10;
    public float overcomeThreshold = 20F;
    public int jumpForgiveness = 5;
    public int coyoteTime = 10;
    public int slideITicks = 12;
    public int slideCooldown = 30;
    public float slideForce = 25F;
    public float slidePenalty = 7F;
}
    