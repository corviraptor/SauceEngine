using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public CharacterController cc;
    public PlayerSettings player;

    //initializations
    public bool isOnGround = false;
    // player.walkSpeedAdj is the max walking speed after modification by crouching.
    float walkSpeedAdj = 10F;

    bool frictionTimerOn = false;
    bool frictionTimerExecuted = false;

    bool frictionImpossibleTimerOn = false;
    bool groundImpact = false;

    bool jumpExecuted = false;

    bool crouchExited = false;

    bool coyoteTimerOn = false;
    bool coyoteTimerExecuted = false;

    bool isSurfing = false;
    bool isOnSlope = false;
    bool surfExecuted = false;
    bool unsloped = false;
    bool isCrouching = false;
    bool isSliding = false;

    float crouchLerp = 0;
    
    Vector3 velocity;
    Vector3 accelXZ;
    float accelX;
    float accelZ;
    
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "Player";
    }

    void LateUpdate()
    {
        // - YAW ROTATION
        float mouseYaw = player.sens * Input.GetAxis("Mouse X");
        transform.Rotate(0, mouseYaw, 0);

        Vector3 localVelocity = this.transform.InverseTransformVector(velocity);

        //sets player velocity at the end of every frame, then sends info to the event system
        cc.Move(velocity * Time.deltaTime);
        GameEvents.current.playerUpdate(this, localVelocity, accelX, accelZ, transform, cc.height, cc.center);
    }

    void Update()
    {
        // - LOCAL SPACE
        Vector3 xHat = transform.right;
        Vector3 zHat = transform.forward;

        //determines vector being inputted on the XZ plane
        accelZ = Input.GetAxis("Vertical");
        accelX = Input.GetAxis("Horizontal");
        accelXZ = ((accelX * xHat) + (accelZ * zHat)) / 2;
        // locked cursor wizardry
        if (Input.GetButtonDown("Cancel"))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (!isOnGround)
        {
            airControl();
        }

        /* - GROUND TEST 
        Detects if the player is on the ground, if they are on a slope too steep to walk on, and if they are surfing on that slope
        i know i could be using guard clauses for these nested if statements, but they make much more sense to me this way in this applicationsorry! lmao!
        transform.position + cc.center = worldspace center */
        if (Physics.Raycast(transform.position + cc.center, -transform.up, out hit))
        {
            //isOnGround, only triggers if player is within gThreshold of the ground surface and the ground surface isn't too steep
            if (hit.distance <= cc.height / 2 + player.gThreshold && Vector3.Dot(hit.normal, transform.up) > player.maxSlope){
                isOnGround = true;
                coyoteTimerExecuted = false;

                if (!groundImpact){
                    StartCoroutine(frictionImpossibleTimer());
                    groundImpact = true;
                    jumpExecuted = false;
                }
                /*Friction timer activates at decent speeds, essentially stops player from experiencing friction and disables walking 
                for a brief time to give a bigger window for bunnyhopping. player.overcomeThreshold should only activate at bunnyhopping 
                speeds, otherwise when reaching the ground even at slow speeds players wont be able to walk for a few ticks! makes it feel sticky and yucky */
                if (!frictionTimerExecuted && new Vector3(velocity.x, 0, velocity.z).magnitude >= player.overcomeThreshold){
                    StartCoroutine(frictionTimer());
                }
                
            }
            else{
                isOnGround = false;

                //resets friction timers when off the ground
                if (groundImpact){
                    groundImpact = false;
                }
                if (frictionTimerExecuted){
                    frictionTimerExecuted = false;
                }

                //activates coyoteTimer every time player leaves ground
                if (!coyoteTimerExecuted){
                    StartCoroutine(coyoteTimer());
                    coyoteTimerOn = true;
                    coyoteTimerExecuted = true;
                }
            }

            // ground magnetism
            if (hit.distance <= cc.height / 2 + player.gMagThreshold && !jumpExecuted && !isOnGround && !isOnSlope){
                velocity -= Vector3.up * player.gMagnetism;
            }

            // slope detection & surfing
            if (hit.distance <= cc.height / 2 + player.gThreshold + 0.5 && Vector3.Dot(hit.normal, transform.up) < player.maxSlope && velocity.magnitude > player.walkSpeed){
                //player is surfing if they are within the ground check distance but the ground is too steep to trigger the ground check, and they are traveling faster than walking speed
                //vertical speed also counts more towards surfing, so players won't start to slip as easy if theyre surfing right up an incline because thats fun
                isSurfing = true;
                isOnSlope = true;
                unsloped = false;

                //surf lift - lets player to ride at an angle that keeps them from losing speed
                Vector3 surfaceX = Vector3.Cross(transform.up, hit.normal);
                Vector3 surfaceZ = Vector3.Cross(hit.normal, surfaceX);
                velocity += surfaceZ * player.surfLift * Time.deltaTime;
                
                //calls the GameEvents system to invoke the SurfEnter event only once, as soon as the player enters the surfing state
                if (!surfExecuted){
                    surfExecuted = true;
                    GameEvents.current.playerSurfEnter(this);
                }

            }
            else if (hit.distance <= cc.height / 2 + player.gThreshold + 0.5 && Vector3.Dot(hit.normal, transform.up) < player.maxSlope)
            {
                isOnSlope = true;
                isSurfing = false;
                unsloped = false;

                //calls the GameEvents system to invoke the SlipEnter event only once, by using the same variable to allow SurfEnter to be triggered again
                if (surfExecuted)
                {
                    GameEvents.current.playerSlipEnter(this);
                    surfExecuted = false;
                }
            }
            else
            {
                isOnSlope = false;
                isSurfing = false;
                if (!unsloped){

                    //calls the GameEvents system to invoke the Unslope event only once
                    unsloped = true;
                    GameEvents.current.playerUnslope(this);
                }

                if (surfExecuted)
                {
                    //calls the GameEvents system to invoke the Unsurf event and allows SurfEnter to be triggered again
                    GameEvents.current.playerUnsurf(this);
                    surfExecuted = false;
                }
            }
        }


        // - SLIP (prevets players from using the base air control speed to just float up ramps if they aren't surfing because that feels weird)
        if (isOnSlope && !isSurfing && velocity.y > -1)
        {
            Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
            float slip = velocityXZ.magnitude * Time.deltaTime * 20;
            Vector3 slipVector = Vector3.Lerp(-transform.up, hit.normal, 0.4F) * slip;
            velocity += slipVector;
        }

        // - WALKING
        if (!jumpExecuted && !frictionTimerOn && !frictionImpossibleTimerOn && isOnGround && accelXZ.magnitude > 0.1F)
        {
            onWalk();
        }
        else if (!jumpExecuted && !frictionTimerOn && !frictionImpossibleTimerOn && isOnGround && accelXZ.magnitude < 0.1F)
        {
            // - FRICTION
            //only applies friction if a jump isn't in progress and the friction timers aren't active
            onFriction();
        }

        // - JUMP
        if (Input.GetButtonDown("Jump"))
        {
            onJumpInput();
        }

        // - CROUCH
        RaycastHit roofHit;
        if (Input.GetButton("Crouch"))
        {
            onCrouchInput();
            crouchExited = false;
            isCrouching = true;
        }
        //stops player from getting up while in too small of a space. uses BoxCast to prevent player from getting up at the edge of a roof and clipping it
        else if (Physics.BoxCast(transform.position + cc.center - Vector3.up * cc.radius, Vector3.one * cc.radius, transform.up, out roofHit) && roofHit.distance <= player.height / 2 + cc.center.y && isCrouching) 
        {
            onCrouchInput();
            crouchExited = false;
        }
        else if (!crouchExited)
        {
            isCrouching = false;
            onCrouchExit();
            crouchExited = true;
        }

        //gets player back up whilst uncrouched
        cc.center = new Vector3(0, 0.5F * crouchLerp, 0);
        cc.height = Mathf.Lerp(player.height, player.height / 2.0F, crouchLerp);
        if (crouchLerp > 0 && !isCrouching)
        {
            crouchLerp -= Time.deltaTime * 8;
            if (isOnGround && !jumpQueueOn)
            {
                //forces player up to avoid collider expanding into the ground. doesnt activate when player jump is being processed
                cc.enabled = false;
                transform.position = new Vector3(transform.position.x, hit.point.y - cc.center.y + cc.height/2 + 0.1F, transform.position.z);
                cc.enabled = true;
            }
        }

        // - GRAVITY
        if (!isOnGround && velocity.y >= player.terminalVelocity){
            onGravity();
        }
        else if (velocity.y <= player.terminalVelocity){
            Debug.Log("terminal velocity reached");
        } 
    }

    public void airControl(){
        // speed limit
        Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
        float AVproj = Vector3.Dot(velocityXZ, accelXZ * player.airAccel);

        if (AVproj < player.vLimit - (accelXZ.magnitude * Time.deltaTime))
        { // look at how delightfully simple this code is. pure math. no bullshit. now look at the rest of this file and cry
            velocityXZ = velocityXZ + (accelXZ * player.airAccel * Time.deltaTime);
            velocity = new Vector3(velocityXZ.x, velocity.y, velocityXZ.z);
        }
    }

    public void onWalk(){
        Vector3 localWalkDirection = Vector3.ProjectOnPlane(accelXZ, hit.normal).normalized;
        Vector3 localWalkVector = localWalkDirection * walkSpeedAdj;
        velocity = localWalkVector;
    }

    public void onFriction(){
        //applies friction
        velocity = new Vector3(velocity.x * player.frictionFactor, velocity.y * player.frictionFactor, velocity.z * player.frictionFactor);
    }

    IEnumerator frictionTimer(){
        frictionTimerExecuted = true;
        frictionTimerOn = true;
        int i = 0;
        while (i <= player.frictionForgiveness){
            i++;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        frictionTimerOn = false;
        i = 0;
    }

    IEnumerator frictionImpossibleTimer(){
        frictionImpossibleTimerOn = true;
        int i = 0;
        while (i <= 2){
            i++;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        frictionImpossibleTimerOn = false;
        i = 0;
    }

    bool jumpQueueOn = false;
    public void onJumpInput(){

        if (!jumpQueueOn){
            StartCoroutine(jumpQueue());
        }
        
        if (!jumpQueueOn || jumpExecuted || isCrouching){
            return;
        }
        if (coyoteTimerOn){
            jumpExecuted = true;
            velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
            GameEvents.current.playerJump(this);
        }
        if (Vector3.Dot(hit.normal, transform.up) > player.maxSlope && isOnGround){
            jumpExecuted = true;
            velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
            GameEvents.current.playerJump(this);
        }
    }

    IEnumerator jumpQueue(){
        jumpQueueOn = true;
        int i = 0;
        while (i <= player.jumpForgiveness){
            i++;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            onJumpInput();
        }
        jumpQueueOn = false;
        i = 0;
    }

    IEnumerator coyoteTimer(){
        /*coyote time's primary purpose here is to prevent very brief moments off the ground from preventing the player to jump, like walking 
        over a sharp bump. being able to jump right after you fall off of a platform doesnt feel that bad in first person, at least to me */
        coyoteTimerExecuted = true;
        coyoteTimerOn = true;
        int i = 0;
        while (i <= player.coyoteTime){
            i++;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        coyoteTimerOn = false;
        i = 0;
    }

    public void onCrouchInput(){
        cc.center = new Vector3(0, 0.5F * crouchLerp, 0);
        cc.height = Mathf.Lerp(player.height, player.height / 2.0F, crouchLerp);
        walkSpeedAdj = player.walkSpeed / 2.0F;

        if (crouchLerp < 1)
        {
            crouchLerp += Time.deltaTime * 7;
            if (isOnGround)
            {
                //when the player is on the ground, this moves them down when crouching to keep their feet at the same level, essentially meaning they just duck instead of having to fall after picking up their feet
                cc.enabled = false;
                transform.position = new Vector3(transform.position.x, hit.point.y - cc.center.y + cc.height/2 + 0.05F, transform.position.z);
                cc.enabled = true;
            }
        }
        
    }
    
    public void onCrouchExit(){
        walkSpeedAdj = player.walkSpeed;
    }

    public void onGravity(){
        RaycastHit floorHit;
        Vector3 gravityVector = new Vector3(0, player.gravity * Time.deltaTime, 0);

        //only adds the fall multiplier if player is falling and close to standable ground to let people soar through the air
        if (Physics.Raycast(transform.position + cc.center, -transform.up, out floorHit)){
            if (velocity.y < 0 && !isSurfing && floorHit.distance <= player.groundPull && Vector3.Dot(floorHit.normal, transform.up) > player.maxSlope){
                gravityVector = gravityVector * player.fallMultiplier;
            }
        }
        
        velocity = velocity - gravityVector;
    }

    void OnControllerColliderHit(ControllerColliderHit cHit){
        //this code allows source engine surfing to happen whithout using rigidbody physics - in fact, i totally just copied it from the source engine's source code! lmao! thanks heckteck for helping me understand this bit
        float backoff = Vector3.Dot(velocity, cHit.normal);
        velocity = velocity - (backoff * cHit.normal);
    }
}