using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    const float TAU = Mathf.PI * 2;

    public CharacterController cc;
    public controlCfg cfg;
    public playerSettings player;

    //initializations
    public bool isOnGround = false;
    // player.walkLimitAdj is the max walking speed after modification by crouching.
    float walkLimitAdj = 10F;

    bool frictionTimerOn = false;
    int groundTicks = 0;
    bool frictionTimerExecuted = false;

    bool frictionImpossible = false;
    int groundTicks2 = 0;
    bool frictionImpossibleExecuted = false;

    bool jumpTimerOn = false;
    bool jumpQueueOn = false;
    int jumpTicks = 0;
    bool jumpExecuted = false;
    bool jumpCurrently = false;
    int jumpCurrentlyClock = 0;

    bool crouchExited = false;

    bool coyoteTimerOn = false;
    int coyoteTicks = 0;
    bool coyoteTimerExecuted = false;

    Vector3 localWalkVector = Vector3.zero;

    float wasSpeed;
    Vector3 wasDirection;
    Vector3 adjVelocity;

    bool isSurfing = false;
    bool isOnSlope = false;
    bool surfExecuted = false;
    bool unsloped = false;
    bool isCrouching = false;
    bool isSliding = false;

    float crouchLerp = 0;
    
    Vector3 velocity;
    
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "Player";
    }

    void FixedUpdate()
    {

        // - FRICTION TIMER (while timer is ON, friction is OFF)
        if (frictionTimerOn)
        {
            frictionTimerExecuted = true;
            groundTicks++;
        }

        if (groundTicks >= player.frictionForgiveness)
        {
            frictionTimerOn = false;
            groundTicks = 0;
            
        }

        //resets ground ticks when friction timer is off
        if (!frictionTimerOn)
        {
            groundTicks = 0;
        }


        // - FRICTION IMPOSSIBLE TIMER (adds a consistent 1 tick delay to friction in all cases before any friction can be applied to let jumps through cleanly)
        // (yes i know the name sucks)
        if (frictionImpossible)
        {
            frictionImpossibleExecuted = true;
            groundTicks2++;
        }

        if (groundTicks2 >= 1)
        {
            
            frictionImpossible = false;
            groundTicks2 = 0;

        }
        

        // - JUMP QUEUE TIMER (while timer is ON, player will jump as soon as reaching the ground, allowing some slightly less precise inputs)
        if (jumpTimerOn)
        {
            onJumpInput();
            jumpTicks++;
        }

        if (jumpTicks >= player.jumpForgiveness)
        {
            jumpQueueOn = false;
        }

        if (jumpTicks >= player.jumpCooldown)
        {
            jumpTimerOn = false;
            jumpTicks = 0;
            //sets jumpExecuted to false to refresh jump ability
            jumpExecuted = false;

        }

        //resets jump ticks when jump timer is off
        if (!jumpTimerOn)
        {
            jumpTicks = 0;

        }

        // - OTHER JUMP THING (applies velocity change over multiple frames, making absolutely sure that it gets applied consistently)
        if (jumpCurrently && jumpCurrentlyClock < 3)
        {
            ++jumpCurrentlyClock;
        }
        else
        {
            jumpCurrently = false;
            jumpCurrentlyClock = 0;
        }

        /* - COYOTE TIME TIMER (while timer is ON, player can jump even while in mid-air. timer remains on for a few ticks after leaving ground)
        coyote time actually serves a slightly different purpose in this application. in first person shooters it doesnt feel that bad to not have that forgiveness,
        however i don't want little bits of geometry or other imperceptibly brief ungroundednessments to eat inputs randomly if i can help it! 
        coyote time only activates if jumpExecuted is false, just to cover my bases against hypothetical edge cases */
        if (coyoteTimerOn)
        {
            coyoteTicks++;
        }
        if (coyoteTicks >= player.coyoteTime || jumpTimerOn)
        {
            coyoteTimerOn = false;
            coyoteTicks = 0;
        }
    }

    void LateUpdate()
    {
        // - YAW ROTATION
        float mouseYaw = cfg.sens * Input.GetAxis("Mouse X");

        transform.Rotate(0, mouseYaw, 0);
    }

    void Update()
    {
        // locked cursor wizardry
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // - LOCAL SPACE
        Vector3 xHat = transform.right;
        Vector3 yHat = transform.up;
        Vector3 zHat = transform.forward;

        // - AIR CONTROL
        float accelZ = player.moveSpeed * Input.GetAxis("Vertical");
        float accelX = player.moveSpeed * Input.GetAxis("Horizontal");
        Vector3 accelXZ = ((accelX * xHat) + (accelZ * zHat)); //needs some work, diagonal inputs just add the 2 together so the magnitudes are higher
        Vector3 velocityXZ;

        // speed limit
        velocityXZ = new Vector3(velocity.x, 0, velocity.z);
        float AVproj = Vector3.Dot(velocityXZ, accelXZ);

        if (!isOnGround)
        {
            if (AVproj < player.vLimit - (accelXZ.magnitude * Time.deltaTime))
            { // look at how delightfully simple this code is. pure math. no bullshit. now look at the code for walking and cry
                velocityXZ = velocityXZ + (accelXZ * Time.deltaTime);
                velocity = new Vector3(velocityXZ.x, velocity.y, velocityXZ.z);
            }
        }

        /* - GROUND TEST 
        i know i could be using guard clauses for these nested if statements, but they make much more sense to me this way in this application
        and i dont want to figure out how to refactor this stuff all into their own functions. sorry! lmao!
        transform.position + cc.center = worldspace center */
        if (Physics.Raycast(transform.position + cc.center, -transform.up, out hit))
        {
            if (hit.distance <= cc.height / 2 + player.gThreshold && Vector3.Dot(hit.normal, transform.up) > player.maxSlope)
            {
                isOnGround = true;
                coyoteTimerExecuted = false;

                /*Friction timer activates at decent speeds, essentially stops player from experiencing friction and disables walking 
                for a brief time to give a bigger window for bunnyhopping. player.overcomeThreshold should only activate at bunnyhopping 
                speeds, otherwise when reaching the ground even at slow speeds players wont be able to walk for a few ticks! makes it feel sticky and yucky */
                if (!frictionTimerExecuted && velocityXZ.magnitude >= player.overcomeThreshold)
                {
                    frictionTimerOn = true;
                }

                if (!frictionImpossibleExecuted)
                {
                    frictionImpossible = true;
                }
                
            }
            else
            {
                isOnGround = false;
                frictionTimerExecuted = false;
                frictionImpossibleExecuted = false;

                if (!coyoteTimerExecuted)
                {
                    coyoteTimerOn = true;
                    coyoteTimerExecuted = true;
                }
            }

            if (hit.distance <= cc.height / 2 + player.gThreshold + 0.5 && Vector3.Dot(hit.normal, transform.up) < player.maxSlope && velocity.magnitude > player.walkLimit - 1)
            {
                //player is surfing if they are within the ground check distance but the ground is too steep to trigger the ground check, and they are traveling faster than walking speed
                //vertical speed also counts more towards surfing, so players won't start to slip as easy if theyre surfing right up an incline because thats fun
                isSurfing = true;
                isOnSlope = true;
                unsloped = false;
                if (!surfExecuted)
                {
                    surfExecuted = true;
                    GameEvents.current.playerSurfEnter(this);
                }

            }
            else if (hit.distance <= cc.height / 2 + player.gThreshold + 0.5 && Vector3.Dot(hit.normal, transform.up) < player.maxSlope)
            {
                isOnSlope = true;
                isSurfing = false;
                unsloped = false;

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
                    unsloped = true;
                    GameEvents.current.playerUnslope(this);
                }

                if (surfExecuted)
                {
                    GameEvents.current.playerUnsurf(this);
                    surfExecuted = false;
                }
            }
        }


        // - SLIP (prevets players from using the base air control speed to just float up ramps if they aren't surfing because that feels weird)
        if (isOnSlope && !isSurfing && velocity.y > -1)
        {
            float slip = velocityXZ.magnitude * Time.deltaTime * 20;
            Vector3 slipVector = Vector3.Lerp(-transform.up, hit.normal, 0.4F) * slip;
            velocity = velocity + slipVector;
        }


        Vector3 inheretedVelocity = Vector3.zero;
        // - WALKING
        if (!frictionTimerOn && isOnGround && !jumpExecuted && !frictionImpossible)
        {
            if (hit.rigidbody != null)
            {
                inheretedVelocity = hit.rigidbody.velocity;
            }
            else
            {
                inheretedVelocity = Vector3.zero;
            }
            if (velocityXZ.magnitude < walkLimitAdj)
            {
                Vector3 walkVector = new Vector3(accelXZ.normalized.x * player.walkSpeed, velocity.y, accelXZ.normalized.z * player.walkSpeed);

                Vector3 localWalkDirection = Vector3.Lerp(walkVector.normalized, -hit.normal, 0.5F);
                localWalkVector = walkVector.magnitude * localWalkDirection;
                Vector3 localWalkVectorAdj = localWalkVector;

                velocity = (localWalkVectorAdj * Time.deltaTime) + inheretedVelocity + Vector3.Lerp(velocity, localWalkDirection, 30 * Time.deltaTime);
            }
        }

        // - FRICTION
        bool applyFriction = false;
        //only applies friction if a jump isn't in progress and the friction timers aren't active
        if (!jumpExecuted && !frictionTimerOn && !frictionImpossible)
        {
            applyFriction = true;
        }
        else
        {
            applyFriction = false;
        }

        if (applyFriction && isOnGround)
        {
            //applies friction
            velocity = new Vector3(velocity.x * player.frictionFactor, velocity.y * player.frictionFactor, velocity.z * player.frictionFactor);

            /*detects if the horizontal acceleration vector and the horizontal velocity vector are pointing within 1/4 turn of eachother, 
            also returns false if the acceleration vector isn't in use. essentially here to prevent strong friction from existing when walking*/
            bool isAdditive = false;
            if (Vector3.Dot(accelXZ.normalized, velocityXZ.normalized) > 0 && accelXZ.magnitude > 0.01F)
            {
                isAdditive = true;
            }
            else
            {
                isAdditive = false;
            }

            /*strong friction - brings player to a stop if they are traveling below a certain horizontal velocity by adding a strong horizontal friction force 
            every tick in addition to the main friction force, but only if isAdditive is false that way if a player is just walking the strong friction isn't applied. 
            will stop being applied when player approximately comes to a stop*/
            if (velocityXZ.magnitude < player.frictionCutoff && !isAdditive && velocityXZ.magnitude > 0.01F)
            {
                velocity = new Vector3(velocity.x * player.frictionFactor * 0.98F, velocity.y * player.frictionFactor * 0.98F, velocity.z * player.frictionFactor * 0.98F);
            }
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
        else if (Physics.Raycast(transform.position + cc.center, transform.up, out roofHit) && roofHit.distance <= player.height && isCrouching) //stops player from getting up while in too small of a space
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

        //gets player back up after crouching
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
        if (!isOnGround && velocity.y <= player.terminalVelocity){
            onGravity();
        }
        else if (velocity.y <= -player.terminalVelocity){
            Debug.Log("terminal velocity reached");
        } 

        //sets player velocity every frame, then sends info to the event system
        cc.Move(velocity * Time.deltaTime);
        Vector3[] playerUpdateVectors = {velocity, accelXZ};
        GameEvents.current.playerUpdate(this, playerUpdateVectors);
    }

    public void onJumpInput(){

        if (!jumpTimerOn){
            jumpTimerOn = true;
            jumpQueueOn = true;
        }

        if (jumpQueueOn && !jumpExecuted && !isCrouching){

            if (hit.distance <= cc.height/2 + 0.1 && Vector3.Dot(hit.normal, transform.up) > player.maxSlope  || coyoteTicks >= 1) // first arg is replacement for isOnGround, just tighter to prevent a "floaty" feeling
            {
                jumpExecuted = true;
                velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
                jumpCurrently = true;
                GameEvents.current.playerJump(this);
            }
        }
        if (jumpCurrently) //extends jump force application to prevent inputs from being eaten
        {
            velocity = new Vector3(velocity.x, player.jumpForce, velocity.z);
        }
    }

    public void onCrouchInput(){
        cc.center = new Vector3(0, 0.5F * crouchLerp, 0);
        cc.height = Mathf.Lerp(player.height, player.height / 2.0F, crouchLerp);
        walkLimitAdj = player.walkLimit / 2.0F;

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
        walkLimitAdj = player.walkLimit;
    }

    public void onGravity(){
        RaycastHit floorHit;
        Vector3 gravityVector = new Vector3(0, player.gravity * Time.deltaTime, 0);

        //only adds the fall multiplier if player is falling and close to standable ground to let people soar through the air
        if (velocity.y < 0 && !isSurfing && Physics.Raycast(transform.position + cc.center, -transform.up, out floorHit) && floorHit.distance <= player.groundPull && Vector3.Dot(floorHit.normal, transform.up) > player.maxSlope)
        {
            gravityVector = gravityVector * player.fallMultiplier;
        }
        if (isSurfing)
        {
            gravityVector = gravityVector * player.surfLift;
        }
        velocity = velocity - gravityVector;
    }

    void OnControllerColliderHit(ControllerColliderHit cHit){
        //this code allows source engine surfing to happen whithout using rigidbody physics - in fact, i totally just copied it from the source engine's source code! lmao! thanks heckteck for helping me understand this bit
        float backoff = Vector3.Dot(velocity, cHit.normal);
        velocity = velocity - (backoff * cHit.normal);
    }
}