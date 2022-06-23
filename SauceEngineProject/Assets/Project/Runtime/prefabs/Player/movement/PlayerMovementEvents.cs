using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementEvents : MonoBehaviour, IBlastible
{
    public static PlayerMovementEvents current;
    public GameObject logic;

    public CharacterController cc;
    public PlayerSettings player;
    [SerializeField] private LayerMask layerMask;

    //key name, int for progress
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public Vector3 center;

    public Vector3 velocity;
    public Vector3 oldPosition;
    Vector3 accelXZ;
    RaycastHit hit;

    public bool isOnGround = false;
    bool slideFailPlayed = false;
    int slopeState = 0;
    int crouchState = 0;
    int slideState = 0;

    float temperature;
    float walkSpeedAdj;
    

    void OnEnable(){

        current = this;
        gameObject.tag = "Player";

        clocks.Add("jumpBuffer", 0);
        clocks.Add("coyoteTime", 0);
        clocks.Add("jumpCooldown", 0);
        clocks.Add("postJumpGravity", 0);
        clocks.Add("slideBuffer", 0);
        clocks.Add("slide", 0);
        clocks.Add("blastTime", 0);

        foreach (Behaviour module in logic.GetComponents<Behaviour>()){
            module.enabled = true;
        }
        walkSpeedAdj = player.walkSpeed;
    }

    void onDestroy(){

        foreach (Behaviour module in logic.GetComponents<Behaviour>()){
            module.enabled = false;
        }
    }

    void LateUpdate(){
        // - YAW ROTATION
        float mouseYaw = InputManager.current.lookVector.x;
        transform.Rotate(0, mouseYaw, 0);

        //sets player velocity at the end of every frame, then sends info to the event system
        cc.Move(velocity * Time.deltaTime);

        Vector3 realVelocity = (transform.position - oldPosition) / Time.deltaTime;

        //assigns PlayerArgs instance for PlayerHandler
        PlayerHandler.current.playerArgs.velocity = realVelocity;
        PlayerHandler.current.playerArgs.localVelocity = this.transform.InverseTransformVector(realVelocity);
        PlayerHandler.current.playerArgs.transform = transform;
        PlayerHandler.current.playerArgs.controller = cc;

        oldPosition = transform.position;
    }

    // Update is called once per frame
    void Update(){
        temperature = PlayerHandler.current.playerArgs.temperature;

        center = transform.position + cc.center;
        // locked cursor wizardry
        if (InputManager.current.menu){
            Cursor.lockState = CursorLockMode.None;
        }

        if (InputManager.current.attack){
            Cursor.lockState = CursorLockMode.Locked;
        }

        //input direction
        accelXZ = InputManager.current.naiveAccelXY.x * transform.right + InputManager.current.naiveAccelXY.y * transform.forward;
        if (accelXZ.sqrMagnitude > 1){
            accelXZ = accelXZ.normalized;
        }

        GroundTest();
        SlopeTest();

        // - JUMP
        if (InputManager.current.jump && clocks["jumpCooldown"] == 0){
            StartCoroutine(Clock("jumpBuffer", player.jumpForgiveness));
            StartCoroutine(Clock("frictionTimer", 2));
        }

        if (clocks["jumpBuffer"] <= player.jumpForgiveness && clocks["jumpBuffer"] != 0 && clocks["jumpCooldown"] == 0){
            Jump(this, player, velocity, isOnGround, hit);
        }

        //maintains jump velocity for a few ticks to make sure it goes through
        if (clocks["jumpCooldown"] != 0 && clocks["jumpCooldown"] < 5){
            velocity = velocity.KillY() + (transform.up * player.jumpForce);
        }

        // air
        if (!isOnGround){
            AirControl(this, player, velocity, accelXZ);
            Gravity(this, player, velocity, hit);
        }

        // - CROUCH: 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (InputManager.current.crouched){
            cc.center = Vector3.up * 0.5F;
            cc.height = player.height / 2.0F;
            walkSpeedAdj = player.walkSpeed / 2;

            Crouch(this, player, crouchState, isOnGround, hit);

            crouchState = 2;
        }
        else{
            crouchState = 1;
        }
        
        if (crouchState !=2){
            UncrouchTest();
        }

        // slide
        if (InputManager.current.slide && velocity.sqrMagnitude > 0.01F && clocks["slideBuffer"] == 0){
            StartCoroutine(Clock("slideBuffer", player.jumpForgiveness));
        }

        if (clocks["slide"] !=0){
            Slide();
        }

        // ground magnetism - will push you down perpendicular to a standable surface when youre very close to it to keep you from ramping off tiny things
        if (hit.distance <= cc.height / 2 + player.gMagThreshold && clocks["jumpCooldown"] == 0 && slopeState == 0 && clocks["blastTime"] == 0){
            velocity -= hit.normal * player.gMagnetism;
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
            GameEvents.current.HeatPlayer(this, "Slide");
            GameEvents.current.SoundCommand("Slide", "Play", 0);
            StartCoroutine(Clock("slide", player.slideCooldown));
            Slide();
        }
        else if (clocks["slideBuffer"] <= player.jumpForgiveness && clocks["slideBuffer"] != 0 && clocks["slide"] == 0 && temperature > player.heatLimit && !slideFailPlayed) {
            slideFailPlayed = true;
            GameEvents.current.SoundCommand("SlideFail", "Play", 0);
        }
        else if (clocks["slideBuffer"] == 0){
            slideFailPlayed = false;
        }

        if (slideState == 1){
            //we know we're not in the middle of a slide past here because of this guard statement
            return;
        }

        if (accelXZ.sqrMagnitude > 0.001F){
            Walk(this, player, velocity, accelXZ, hit, walkSpeedAdj);
        }
        else {
            Friction(this, player, velocity);
        }
    }

    public void StartJumpCooldown(){
        if (clocks["jumpCooldown"] == 0){
            StartCoroutine(Clock("jumpCooldown", player.coyoteTime + 10));
            StopCoroutine(Clock("postJumpGravity", 30));
            StartCoroutine(Clock("postJumpGravity", 30));
        }
    }

    void GroundTest(){
        Physics.Raycast(center, -transform.up, out hit, Mathf.Infinity, layerMask);
        if (hit.distance <= cc.height / 2 + player.gThreshold && Vector3.Dot(hit.normal, transform.up) > player.maxSlope){
            if (!isOnGround){ OnGroundEnter(); }
            isOnGround = true;
        }
        else {
            if (isOnGround){ OnGroundExit(); }
            isOnGround = false;
        }
    }

    RaycastHit roofHit;
    void UncrouchTest(){
        //boxcast returns false if nothing was hit, cc.center.y + cc.height / 2 = the distance from the center that needs to be clear to stand up without getting stuck
        if (Physics.BoxCast(center, Vector3.one * cc.radius, Vector3.up, out roofHit, Quaternion.identity, Mathf.Infinity, layerMask) && roofHit.distance < cc.center.y + cc.height / 2){
            //stops player from getting up while in too small of a space. uses BoxCast to prevent player from getting up at the edge of a roof and clipping it
            crouchState = 1;
            Crouch(this, player, crouchState, isOnGround, hit);
        }
        else if (slideState == 1){
            //stops player from getting up while in the middle of a slide
            crouchState = 1;
            Crouch(this, player, crouchState, isOnGround, hit);

        }
        else {
            //this will only be called once, as once crouchState is 0 UncrouchTest will not be called by Update
            walkSpeedAdj = player.walkSpeed;
            cc.center = Vector3.zero;
            cc.height = player.height;

            Crouch(this, player, crouchState, isOnGround, hit);
            crouchState = 0;
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
        else if (accelXZ.sqrMagnitude > 0.25F){
            slideVector = accelXZ.normalized;
        }
        else {
            slideVector = transform.forward;
        }

        if (clocks["jumpCooldown"] != 0 && clocks["slide"] < player.slideITicks){ //slide jump
            slideState = 0;
            if (!slideJumped){
                slideJumped = true;
                GameEvents.current.SoundCommand("SlideJump", "Play", 0);
                GameEvents.current.SoundCommand("Slide", "Stop", 0);
            }

            return;
        }

        if (clocks["slide"] > player.slideITicks || clocks["slide"] == 0){ //end of slide's boost
            slideState = 0;
            slideJumped = false;

            return;
        }

        //we know slide will be executed after this point
        slideState = 1;
        if (velocityXZ.sqrMagnitude < player.slideForce * player.slideForce){
            velocity = (slideVector * player.slideForce) + (Vector3.up * velocity.y);
        }
        else{
            velocity = (slideVector * velocityXZ.magnitude) + (Vector3.up * velocity.y);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        //feet
        Gizmos.DrawSphere(center - Vector3.up * (cc.height / 2) + Vector3.up * cc.radius, cc.radius); 
        //head
        Gizmos.DrawSphere(center + Vector3.up * (cc.height / 2) - Vector3.up * cc.radius, cc.radius);
    }

    bool unsloped = false;
    bool surfed = false;
    bool slipped = false;
    void SlopeTest(){
        if (hit.distance <= cc.height / 2 + player.gMagThreshold && Vector3.Dot(hit.normal, transform.up) < player.maxSlope && velocity.sqrMagnitude > player.walkSpeed * player.walkSpeed){
            /*player is surfing if they are within the ground check distance but the ground is too steep to trigger the ground check, and they are traveling faster than walking speed
            vertical speed also counts more towards surfing, so players won't start to slip as easy if theyre surfing right up an incline because thats fun*/
            slopeState = 2;
            unsloped = false;
            Slope(this, player, velocity, slopeState, hit);
            if (!surfed){
                GameEvents.current.SoundCommand("SurfAttack", "Play", 0);
                GameEvents.current.SoundCommand("SlipLoop", "StopFade", 10);
                GameEvents.current.SoundCommand("SurfLoop", "PlayFade", 10);
                surfed = true;
                slipped = false;
            }
        }
        else if (hit.distance <= cc.height / 2 + player.gMagThreshold + 0.5F && Vector3.Dot(hit.normal, transform.up) < player.maxSlope){
            slopeState = 1;
            unsloped = false;
            Slope(this, player, velocity, slopeState, hit);

            if (surfed && !slipped){
                GameEvents.current.SoundCommand("SlipLoop", "PlayFade", 10);
                GameEvents.current.SoundCommand("SurfLoop", "StopFade", 10);
                surfed = false;
                slipped = true;
            }
        }
        else {
            slopeState = 0;
            if (!unsloped){
                //calls the GameEvents system to invoke the Unslope event only once
                GameEvents.current.SoundCommand("SlipLoop", "StopFade", 10);
                GameEvents.current.SoundCommand("SurfLoop", "StopFade", 10);
                unsloped = true;
            }
            if (slipped){
                GameEvents.current.SoundCommand("SlipRelease", "Play", 10);
                slipped = false;
            }
            if (surfed){
                GameEvents.current.SoundCommand("SurfRelease", "Play", 10);
                surfed = false;
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit cHit){
        //this code allows source engine surfing to happen whithout using rigidbody physics - in fact, i totally just copied it from the source engine's source code! lmao! thanks heckteck for helping me understand this bit
        float backoff = Vector3.Dot(velocity, cHit.normal);
        velocity = velocity - (backoff * cHit.normal);
    }

    void OnGroundEnter(){
        StartCoroutine(Clock("frictionTimer", player.frictionForgiveness));
    }

    void OnGroundExit(){
        StartCoroutine(Clock("coyoteTime", player.coyoteTime));
    }

    public void Blast(object sender, string id, Vector3 blastForceVector){
        StartCoroutine(Clock("blastTime", 10));
        velocity = new Vector3(velocity.x, velocity.y / 2, velocity.z);
        if (Vector3.Dot(velocity, blastForceVector) >= 0){
            velocity += blastForceVector / player.mass;
        }
        else {
            //half horizontal blast force if heading into the blast to prevent all of your momentum from being eaten
            velocity +=  blastForceVector.KillY()/ 4 + (Vector3.up * blastForceVector.y) / player.mass; 
        }

        if (id == "Rocket"){
            GameEvents.current.HeatPlayer(this, "Rocket");
        }
    }



    public event Action<object, PlayerSettings, Vector3, Vector3> OnAirControl;
    public void AirControl(object sender, PlayerSettings player, Vector3 v, Vector3 a){
        if (OnAirControl != null){
            OnAirControl(sender, player, v, a);
        }
    }

    public event Action<object, PlayerSettings, Vector3, RaycastHit> OnGravity;
    public void Gravity(object sender, PlayerSettings player, Vector3 v, RaycastHit hit){
        if (OnGravity != null){
            OnGravity(sender, player, v, hit);
        }
    }

    public event Action<object, PlayerSettings, Vector3> OnFriction;
    public void Friction(object sender, PlayerSettings player, Vector3 v){
        if (OnFriction != null){
            OnFriction(sender, player, v);
        }
    }

    public event Action<object, PlayerSettings, Vector3, Vector3, RaycastHit, float> OnWalk;
    public void Walk(object sender, PlayerSettings player, Vector3 v, Vector3 a, RaycastHit hit, float w){
        if (OnWalk != null){
            OnWalk(sender, player, v, a, hit, w);
        }
    }

    public event Action<object, PlayerSettings, Vector3, bool, RaycastHit> OnJump;
    public void Jump(object sender, PlayerSettings player, Vector3 v, bool isOnGround, RaycastHit hit){
        if (OnJump != null){
            OnJump(sender, player, v, isOnGround, hit);
        }
    }

    public event Action<object, PlayerSettings, Vector3, int, RaycastHit> OnSlope;
    public void Slope(object sender, PlayerSettings player, Vector3 v, int slopeState, RaycastHit hit){
        if (OnSlope != null){
            OnSlope(sender, player, v, slopeState, hit);
        }
    }

    public event Action<object, PlayerSettings, int, bool, RaycastHit> OnCrouch;
    public void Crouch(object sender, PlayerSettings player, int crouchState, bool isOnGround, RaycastHit hit){
        if (OnCrouch != null){
            OnCrouch(sender, player, crouchState, isOnGround, hit);
        }
    }

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

            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        i = 0;
        clocks[id] = i;
    }
}
