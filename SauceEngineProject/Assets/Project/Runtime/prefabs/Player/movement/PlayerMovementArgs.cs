using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementArgs
{
    public Vector3 wishDir;
    public RaycastHit hit;
    public int crouchState;

    public bool isOnGround;
    public bool isOnGrounder; // activates only after the first tick, keeping some stuff from enabling during bunnyhopping
    public bool frictionForgiven;
    public bool slideJumped;
    
    public int slopeState;
    public int slideState;

    public PlayerMovementArgs(Vector3 w, RaycastHit h, int cs, bool iog, bool iogr, bool ff, int s, int sls){
        wishDir = w;
        hit = h;
        crouchState = cs;
        isOnGround = iog;
        isOnGrounder = iogr;
        frictionForgiven = ff;
        slopeState = s;
        slideState = s;
    }
}
