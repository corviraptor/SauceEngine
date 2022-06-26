using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IBlastible
{
    public GameObject logic;
    public PlayerHandler playerHandler;

    public CharacterController cc;
    public PlayerSettings player;
    [SerializeField] private LayerMask layerMask;

    //key name, int for progress
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    public Vector3 center;

    public Vector3 velocity;
    public Vector3 oldPosition;
    public Vector3 accelXZ;
    public RaycastHit hit;
    public int crouchState = 0;

    public bool isOnGround = false;
    public bool frictionForgiven => clocks["frictionTimer"] > 0 && velocity.KillY().magnitude > player.overcomeThreshold;
    
    public int slopeState = 0;
    bool slideFailPlayed = false;
    int slideState = 0;

    float temperature;
    public float walkSpeedAdj;
    

    void OnEnable(){

        gameObject.tag = "Player";

        clocks.Add("jumpBuffer", 0);
        clocks.Add("coyoteTime", 0);
        clocks.Add("jumpCooldown", 0);
        clocks.Add("postJumpGravity", 0);
        clocks.Add("slideBuffer", 0);
        clocks.Add("slide", 0);
        clocks.Add("blastTime", 0);

        foreach (MonoBehaviour module in logic.GetComponents<MonoBehaviour>()){
            if (module is IAttachable){
                IAttachable attachable = (IAttachable)module;
                attachable.InjectDependency(this);
                module.enabled = true;
            }
            else { Debug.Log("HELP!!! In PlayerMovement 56"); }
        }

        walkSpeedAdj = player.walkSpeed;
    }

    void onDestroy(){

        foreach (MonoBehaviour module in logic.GetComponents<MonoBehaviour>()){
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
        playerHandler.playerArgs.velocity = realVelocity;
        playerHandler.playerArgs.localVelocity = this.transform.InverseTransformVector(realVelocity);
        playerHandler.playerArgs.transform = transform;
        playerHandler.playerArgs.controller = cc;

        oldPosition = transform.position;
    }

    // Update is called once per frame
    void Update(){
        temperature = playerHandler.playerArgs.temperature;

        center = transform.position + cc.center;
        // locked cursor wizardry
        if (InputManager.current.menu){
            Cursor.lockState = CursorLockMode.None;
        }

        if (InputManager.current.onAttack){
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
            Jump(this);
        }

        //maintains jump velocity for a few ticks to make sure it goes through
        if (clocks["jumpCooldown"] != 0 && clocks["jumpCooldown"] < 5){
            velocity = velocity.KillY() + (transform.up * player.jumpForce);
        }

        // air
        if (!isOnGround){
            AirControl(this);
            Gravity(this);
        }

        // - CROUCH: 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (InputManager.current.crouched){
            cc.center = Vector3.up * 0.5F;
            cc.height = player.height / 2.0F;
            walkSpeedAdj = player.walkSpeed / 2;

            Crouch(this);

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

        if (clocks["slide"] != 0){
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
            GameEvents.current.HeatPlayer(this, player.slideHeat);
            GameEvents.current.SoundCommand("Slide", "Play", 0);
            StartCoroutine(Clock("slide", player.slideCooldown));
            Slide();
        }
        else if (clocks["slideBuffer"] <= player.jumpForgiveness && clocks["slideBuffer"] != 0 && clocks["slide"] == 0 && temperature > player.heatLimit && !slideFailPlayed) {
            GameEvents.current.SoundCommand("SlideFail", "Play", 0);
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

        if (accelXZ.sqrMagnitude > 0.001F){
            Walk(this);
        }
        else {
            Friction(this);
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
            Crouch(this);
        }
        else if (slideState == 1){
            //stops player from getting up while in the middle of a slide
            crouchState = 1;
            Crouch(this);

        }
        else {
            //this will only be called once, as once crouchState is 0 UncrouchTest will not be called by Update
            walkSpeedAdj = player.walkSpeed;
            cc.center = Vector3.zero;
            cc.height = player.height;

            Crouch(this);
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

        if (clocks["jumpCooldown"] != 0 && clocks["slide"] < player.slideITicks && temperature <= player.heatLimit){ //slide jump
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

        if (temperature > player.heatLimit){
            // no slide redirection or anything if over heat limit, but slide state stays to stop friction and walking from eating your momentum
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
            Slope(this);
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
            Slope(this);

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

    Vector3 adjustedForceVector;
    public void Blast(object sender, string id, Vector3 blastForceVector){
        StartCoroutine(Clock("blastTime", 10));
        if (Vector3.Dot(velocity, -blastForceVector) >= 0.5f){
			//half horizontal blast force if heading into the blast to prevent all of your momentum from being eaten
            adjustedForceVector =  blastForceVector.KillY()/ 4 + (Vector3.up * blastForceVector.y) / player.mass; 
        }
        else {
            adjustedForceVector = blastForceVector / player.mass;
        }

        if (velocity.y < 0){
            velocity -= Vector3.up * (velocity.y / 2); // dulls the existing vertical momentum a little if falling downards
        }
        velocity += adjustedForceVector;

        if (sender is Rocket){
            GameEvents.current.HeatPlayer(this, Mathf.Clamp(adjustedForceVector.magnitude * player.rocketJumpHeatFactor, 0, 20F));
        }
    }

    public event Action<object> OnAirControl;
    public void AirControl(object sender){ OnAirControl?.Invoke(sender); }

    public event Action<object> OnGravity;
    public void Gravity(object sender){ OnGravity?.Invoke(sender); }

    public event Action<object> OnFriction;
    public void Friction(object sender){ OnFriction?.Invoke(sender); }

    public event Action<object> OnWalk;
    public void Walk(object sender){ OnWalk?.Invoke(sender); }

    public event Action<object> OnJump;
    public void Jump(object sender){ OnJump?.Invoke(sender); }

    public event Action<object> OnSlope;
    public void Slope(object sender){ OnSlope?.Invoke(sender); }

    public event Action<object> OnCrouch;
    public void Crouch(object sender){ OnCrouch?.Invoke(sender); }

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
