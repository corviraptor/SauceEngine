using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Freya;

public class PlayerMovement : MonoBehaviour, IBlastible, IPlayerHandlerModule
{
    public GameObject logic;
    PlayerHandler playerHandler;

    public PlayerArgs pargs;
    public PlayerMovementArgs margs;

    FirstPersonActions.PlayerActions input => InputManager.current.input;

    public CharacterController cc;
    public PlayerSettings player;
    [SerializeField] private LayerMask layerMask;

    //key name, int for progress
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public Vector3 velocity;
    public Vector3 realVelocity;
    public Vector3 center;
    Vector3 wishDir;
    RaycastHit hit;
    int crouchState = 0;

    bool isOnGround = false;
    bool isOnGrounder = false; // activates only after the first tick, keeping some stuff from enabling during bunnyhopping
    bool frictionForgiven => clocks["groundTimer"] > 0 && velocity.KillY().sqrMagnitude > player.overcomeThreshold * player.overcomeThreshold;
    
    int slopeState = 0;
    bool slideFailPlayed = false;
    int slideState = 0;

    float temperature;
    public float walkSpeedAdj;
    

    public void InjectDependency(PlayerHandler ph){
        playerHandler = ph;
        pargs = playerHandler.playerArgs;

        InputManager.current.OnPressButtons += OnPressButtons;
        
        gameObject.tag = "Player";

        /*if youre asking why this is cleaner to me than just having everything use its own coroutine, its nice to have everything use the same system
        and having everything have accessible ticks so you can do logic based on the progress of the clocks and not just when theyre finished is really convenient */
        string[] initializedClocks = {"jumpBuffer", "coyoteTime", "jumpCooldown", "slideBuffer", "slide", "blastTime", "groundTimer"};
        foreach (string s in initializedClocks){
            clocks.Add(s, 0);
        }

        foreach (MonoBehaviour module in logic.GetComponents<MonoBehaviour>()){
            if (module is IAttachable){
                IAttachable attachable = (IAttachable)module;
                attachable.InjectDependency(this);
                module.enabled = true;
            }
            else { Debug.Log("HELP!!! In PlayerMovement OnEnable()"); }
        }

        walkSpeedAdj = player.walkSpeed;
    }

    void onDestroy(){
        InputManager.current.OnPressButtons -= OnPressButtons;

        foreach (MonoBehaviour module in logic.GetComponents<MonoBehaviour>()){
            module.enabled = false;
        }
    }

    void OnPressButtons(Dictionary<string, bool> buttons){
        if (buttons["jump"] && clocks["jumpCooldown"] == 0){
            StartCoroutine(Clock("jumpBuffer", player.jumpForgiveness));
            StartCoroutine(Clock("groundTimer", 2));
        }

        if (buttons["slide"] && clocks["slideBuffer"] == 0){
            StartCoroutine(Clock("slideBuffer", player.jumpForgiveness));
        }
    }

    Vector3 oldPosition;
    void LateUpdate(){
        if (PauseMenu.isPaused){ return; }

        // - YAW ROTATION
        float mouseYaw = InputManager.current.lookVector.x;
        transform.Rotate(0, mouseYaw, 0);

        //sets player velocity at the end of every frame, then sends info to the event system
        cc.Move(velocity * Time.deltaTime);

        realVelocity = (transform.position - oldPosition) / Time.deltaTime;

        //assigns PlayerArgs instance for PlayerHandler
        playerHandler.playerArgs.velocity = realVelocity;
        playerHandler.playerArgs.localVelocity = this.transform.InverseTransformVector(realVelocity);
        playerHandler.playerArgs.transform = transform;
        playerHandler.playerArgs.center = center;
        playerHandler.playerArgs.controller = cc;

        pargs = playerHandler.playerArgs;
        margs = new PlayerMovementArgs(wishDir, hit, crouchState, isOnGround, isOnGrounder, frictionForgiven, slopeState);

        oldPosition = transform.position;
    }

    void Update(){
        if (PauseMenu.isPaused){ return; }
        if (margs == null){ return; }

        temperature = playerHandler.playerArgs.temperature;

        center = transform.position + cc.center;

        //input direction
        wishDir = InputManager.current.naiveAccelXY.x * transform.right + InputManager.current.naiveAccelXY.y * transform.forward;
        if (wishDir.sqrMagnitude > 1){
            wishDir = wishDir.normalized;
        }

        try {
            Physics.Raycast(center, -transform.up, out hit, Mathf.Infinity, layerMask);
        }
        catch {
            Debug.Log("Out of bounds!");
            transform.position = Vector3.zero;
            return;
        }

        GroundTest();
        SlopeTest();

        // - JUMP 
        if (clocks["jumpBuffer"] <= player.jumpForgiveness && clocks["jumpBuffer"] != 0 && clocks["jumpCooldown"] == 0){
            Jump();
        }

        //maintains jump velocity for a few ticks to make sure it goes through
        if (clocks["jumpCooldown"] != 0 && clocks["jumpCooldown"] < 2){
            velocity = velocity.KillY() + (transform.up * player.jumpForce);
        }

        //start accelerating player down after a jump to assist gravity to make jumping feel more chunky. dont do this if player was blasted!
        if (clocks["jumpCooldown"] != 0 && clocks["jumpCooldown"] > 17 && !slideJumped && clocks["blastTime"] == 0){
            velocity -= transform.up * player.jumpForce * Time.deltaTime * player.postJumpPushDown;
        }

        // air
        if (!isOnGrounder){
            AirControl();
            Gravity();
        }

        // - CROUCH: 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (input.Crouch.ReadValue<float>() != 0){
            Crouch();
            crouchState = 2;
        }
        else{
            crouchState = 1;
        }
        
        if (crouchState == 1){
            UncrouchTest();
        }

        // slide
        if (clocks["slide"] != 0){
            Slide();
        }

        if (!isOnGround){
            // we know we're on the ground if we get past here because of this guard statement
            return;
        }

        if (clocks["blastTime"] != 0){
            // we know we havent just blast jumped past here because of this guard statement
            return;
        }
        
        // more slide logic
        if (clocks["slideBuffer"] <= player.jumpForgiveness && clocks["slideBuffer"] != 0 && clocks["slide"] == 0 && temperature <= player.heatLimit){
            GameEvents.current.HeatPlayer(this, player.slideHeat);
            playerHandler.SoundCommand("Slide", "Play", 0);
            StartCoroutine(Clock("slide", player.slideCooldown));
            Slide();
        }
        else if (clocks["slideBuffer"] <= player.jumpForgiveness && clocks["slideBuffer"] != 0 && clocks["slide"] == 0 && temperature > player.heatLimit && !slideFailPlayed) {
            playerHandler.SoundCommand("SlideFail", "Play", 0);
            slideFailPlayed = true;
            StartCoroutine(Clock("slide", player.slideCooldown));
        }
        else if (clocks["slideBuffer"] == 0){
            slideFailPlayed = false;
        }

        if (slideState == 1){
            //we know we're not in the middle of a slide past here because of this guard statement
            return;
        }

        if (!isOnGrounder){
            //we know we've been on the ground longer than 1 tick if we get past this guard statement
            return;
        }

        /* ground magnetism - will push you down perpendicular to a standable surface when youre very close to it to keep you from ramping off tiny bumps
        its down here so that you dont get magnetized to the ground when surfing off of a slope that connects to flat ground */
        if (hit.distance <= cc.height / 2 + player.gMagThreshold && clocks["jumpCooldown"] == 0 && slopeState == 0 && clocks["blastTime"] == 0){
            velocity -= hit.normal * player.gMagnetism;
        }

        if (wishDir.sqrMagnitude > 0.001F){
            Walk();
        }
        else {
            Friction();
        }
    }

    public void StartJumpCooldown(){
        playerHandler.SoundCommand("Jump", "Play", 0);
        if (clocks["jumpCooldown"] == 0){
            StartCoroutine(Clock("jumpCooldown", player.coyoteTime + 5));
        }
    }

    float newMaxSlope;
    void GroundTest(){
        newMaxSlope = player.maxSlope;

        if (velocity.KillY().magnitude > player.overcomeThreshold + 10){
            //player wont be able to walk on some slopes at high speeds so that you can surf up stairs and shit like in tf2
            newMaxSlope = player.maxSlope + ((1 - player.maxSlope) / 1.5F);
        }

        if (hit.distance <= cc.height / 2 + player.gThreshold && Vector3.Dot(hit.normal, transform.up) > newMaxSlope){
            if (!isOnGround){ OnGroundEnter(); }
            if (clocks["groundTimer"] > 1){ isOnGrounder = true; }
            isOnGround = true;
        }
        else if (hit.distance <= cc.height / 2 + player.gThreshold && hit.collider.tag == "Stairs" && newMaxSlope == player.maxSlope){
            //can walk up steeper slopes if they are tagged as stairs, but only if maxSlope isnt adjusted
            if (!isOnGround){ OnGroundEnter(); }
            if (clocks["groundTimer"] > 1){ isOnGrounder = true; }
            isOnGround = true;
        }
        else {
            if (isOnGround){ OnGroundExit(); }
            isOnGround = false;
            isOnGrounder = false;
        }
    }

    bool isOnSlope = false;
    bool unsloped = false;
    bool surfed = false;
    bool slipped = false;
    void SlopeTest(){

        if (hit.collider.tag == "Stairs" && newMaxSlope == player.maxSlope){
            //can walk up steeper slopes if they are tagged as stairs, but only if maxSlope isnt adjusted
            isOnSlope = false;
        }
        else if (hit.distance <= cc.height / 2 + player.gMagThreshold && Vector3.Dot(hit.normal, transform.up) < newMaxSlope){
            isOnSlope = true;
        }
        else {
            isOnSlope = false;
        }

        if (isOnSlope && velocity.sqrMagnitude > player.walkSpeed * player.walkSpeed){
            /*player is surfing if they are within the ground check distance but the ground is too steep to trigger the ground check, and they are traveling faster than walking speed
            vertical speed also counts more towards surfing, so players won't start to slip as easy if theyre surfing right up an incline because thats fun*/
            slopeState = 2;
            unsloped = false;
            Slope();
            if (!surfed){
                playerHandler.SoundCommand("SurfAttack", "Play", 0);
                playerHandler.SoundCommand("SlipLoop", "StopFade", 10);
                playerHandler.SoundCommand("SurfLoop", "PlayFade", 10);
                surfed = true;
                slipped = false;
            }
        }
        else if (isOnSlope){
            //slipping
            slopeState = 1;
            unsloped = false;
            Slope();

            if (surfed && !slipped){
                playerHandler.SoundCommand("SlipLoop", "Play", 0);
                playerHandler.SoundCommand("SurfLoop", "StopFade", 10);
                surfed = false;
                slipped = true;
            }
        }
        else {
            slopeState = 0;
            if (!unsloped){
                //calls the GameEvents system to invoke the Unslope event only once
                playerHandler.SoundCommand("SlipLoop", "StopFade", 10);
                playerHandler.SoundCommand("SurfLoop", "StopFade", 10);
                unsloped = true;
            }
            if (slipped){
                playerHandler.SoundCommand("SlipRelease", "Play", 10);
                slipped = false;
            }
            if (surfed){
                playerHandler.SoundCommand("SurfRelease", "Play", 10);
                surfed = false;
            }
        }
    }

    RaycastHit roofHit;
    void UncrouchTest(){
        //boxcast returns false if nothing was hit, cc.center.y + cc.height / 2 = the distance from the center that needs to be clear to stand up without getting stuck
        if (Physics.BoxCast(center, Vector3.one * cc.radius, Vector3.up, out roofHit, Quaternion.identity, Mathf.Infinity, layerMask) && roofHit.distance < cc.center.y + cc.height / 2){
            //stops player from getting up while in too small of a space. uses BoxCast to prevent player from getting up at the edge of a roof and clipping it
            crouchState = 1;
            Crouch();
        }
        else if (slideState == 1){
            //stops player from getting up while in the middle of a slide
            crouchState = 1;
            Crouch();

        }
        else {
            //this will only be called once, as once crouchState is 0 UncrouchTest will not be called by Update
            crouchState = 0;
            Crouch();
        }
    }  

    bool slideJumped = false;
    void Slide(){ //i know its weird that slide doesn't have its own script but it'd be a hassle to try and manage the timers in another script and it isnt that big so whatever
        Vector3 velocityXZ = velocity.KillY();
        Vector3 slideVector;

        if (clocks["slide"] != 1){
            //slide should only change the player's velocity direction at the beginning of the slide, to prevent weirdness of slide angle being adjusted mid-slide
            slideVector = velocityXZ.normalized;
        }
        else if (wishDir.sqrMagnitude > 0.25F){
            slideVector = wishDir.normalized;
        }
        else {
            slideVector = transform.forward;
        }

        if (clocks["jumpCooldown"] != 0 && clocks["slide"] < player.slideDuration && temperature <= player.heatLimit){ //slide jump
            slideState = 0;
            if (!slideJumped){
                slideJumped = true;
                playerHandler.SoundCommand("SlideJump", "Play", 0);
                playerHandler.SoundCommand("Slide", "Stop", 0);
            }

            return;
        }

        if (clocks["slide"] > player.slideDuration || clocks["slide"] == 0){ //end of slide's boost
            slideState = 0;
            slideJumped = false;

            return;
        }

        if (temperature > player.heatLimit){
            // no slide redirection or anything if over heat limit, but slide state stays to stop friction and walking from eating your momentum
            return;
        }

        //we know slide will be executed after this point
        slideState = 1;
        crouchState = 2;
        Crouch();
        if (velocityXZ.sqrMagnitude < player.slideForce * player.slideForce){
            velocity = (slideVector * player.slideForce);
        }
        else{
            /* you will get a penalty in speed if slide jumping past slide's natural speed boost and you won't entirely redirect your momentum
            because honestly i started to not like how that felt, plus it makes grapple hook less interesting */
            velocity = Vector3.Lerp(velocityXZ, (slideVector * player.slideForce), 0.5F);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        //feet
        Gizmos.DrawSphere(center - Vector3.up * (cc.height / 2) + Vector3.up * cc.radius, cc.radius); 
        //head
        Gizmos.DrawSphere(center + Vector3.up * (cc.height / 2) - Vector3.up * cc.radius, cc.radius);
    }

    void OnControllerColliderHit(ControllerColliderHit cHit){
        //this code allows source engine surfing to happen whithout using rigidbody physics - in fact, i totally just copied it from the source engine's source code! lmao! thanks heckteck for helping me understand this bit
        float backoff = Vector3.Dot(velocity, cHit.normal);
        velocity = velocity - (backoff * cHit.normal);
    }

    void OnGroundEnter(){
        StartCoroutine(Clock("groundTimer", player.frictionForgiveness));
    }

    void OnGroundExit(){
        StartCoroutine(Clock("coyoteTime", player.coyoteTime));
    }

    Vector3 adjustedForceVector;
    public void Blast(object sender, string id, Vector3 blastForceVector){
        StartCoroutine(Clock("blastTime", 10));

        if (Vector3.Dot(velocity, -blastForceVector) >= 0.5f){
			//dull horizontal blast force if heading into the blast to prevent all of your momentum from being eaten.
            adjustedForceVector =  (blastForceVector.KillY() / 2) + (Vector3.up * blastForceVector.y) / player.mass; 
        }
        else {
            adjustedForceVector = blastForceVector / player.mass;
        }

        velocity += adjustedForceVector.normalized * Mathfs.Clamp(adjustedForceVector.magnitude, 0, 20);

        if (sender is Rocket){
            GameEvents.current.HeatPlayer(this, Mathf.Clamp(adjustedForceVector.magnitude * player.rocketJumpHeatFactor, 0, 20F));
        }
    }

    public event Action OnAirControl;
    public void AirControl(){ OnAirControl?.Invoke(); }

    public event Action OnGravity;
    public void Gravity(){ OnGravity?.Invoke(); }

    public event Action OnFriction;
    public void Friction(){ OnFriction?.Invoke(); }

    public event Action OnWalk;
    public void Walk(){ OnWalk?.Invoke(); }

    public event Action OnJump;
    public void Jump(){ OnJump?.Invoke(); }

    public event Action OnSlope;
    public void Slope(){ OnSlope?.Invoke(); }

    public event Action OnCrouch;
    public void Crouch(){ OnCrouch?.Invoke(); }

    IEnumerator Clock(string id, int interval){
        int temp;
        //adds clock if it doesn't exist already
        if(!clocks.TryGetValue(id, out temp)){
            clocks.Add(id, 0);
        }
        int i = 0;
        while (i <= interval){
            i++;
            clocks[id] = i;

            yield return new WaitForFixedUpdate();
        }
        i = 0;
        clocks[id] = i;
    }
}