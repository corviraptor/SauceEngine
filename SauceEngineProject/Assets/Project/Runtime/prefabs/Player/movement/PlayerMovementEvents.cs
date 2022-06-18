using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementEvents : MonoBehaviour
{
    public static PlayerMovementEvents current;
    public GameObject logic;

    public CharacterController cc;
    public PlayerSettings player;
    [SerializeField] private LayerMask layerMask;

    //key name, bool for if its complete, int for progress
    public Dictionary<string, int> clocks = new Dictionary<string, int>();

    Vector3 center;

    public Vector3 velocity;
    Vector3 accelXZ;
    RaycastHit hit;
    public bool isOnGround;
    bool crouchExited;
    int slopeState = 0;
    int crouchState = 0;
    int slideState = 0;

    float heat;
    float walkSpeedAdj;

    //temp
    float accelX;
    float accelZ;
    

    void Start(){
        current = this;
        gameObject.tag = "Player";
        GameEvents.current.OnHeatUpdate += HeatUpdate;
        clocks.Add("jumpBuffer", 0);
        clocks.Add("coyoteTime", 0);
        clocks.Add("jumpCooldown", 0);
        clocks.Add("slideBuffer", 0);
        clocks.Add("slide", 0);

        foreach (Behaviour module in logic.GetComponents<Behaviour>()){
            module.enabled = true;
        }

        walkSpeedAdj = player.walkSpeed;
    }

    void onDestroy(){
        GameEvents.current.OnHeatUpdate -= HeatUpdate;
        
        foreach (Behaviour module in logic.GetComponents<Behaviour>()){
            module.enabled = false;
        }
    }

    void LateUpdate(){
        // - YAW ROTATION
        float mouseYaw = player.sens * Input.GetAxis("Mouse X");
        transform.Rotate(0, mouseYaw, 0);

        //sets player velocity at the end of every frame, then sends info to the event system
        cc.Move(velocity * Time.deltaTime);

        Vector3 localVelocity = this.transform.InverseTransformVector(velocity);
        GameEvents.current.playerUpdate(this, localVelocity, accelX, accelZ, transform, player.height, cc.center, isOnGround);
    }

    void HeatUpdate(object sender, float h){
        heat = h;
    }

    // Update is called once per frame
    void Update(){
        center = transform.position + cc.center;
        // locked cursor wizardry
        if (Input.GetButtonDown("Cancel")){
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetButtonDown("Fire1")){
            Cursor.lockState = CursorLockMode.Locked;
        }

        //input direction
        accelZ = Input.GetAxis("Vertical");
        accelX = Input.GetAxis("Horizontal");
        accelXZ = (accelX * transform.right + accelZ * transform.forward);
        if (accelXZ.magnitude > 1){
            accelXZ = accelXZ.normalized;
        }

        GroundTest();
        SlopeTest();

        // - JUMP
        if (Input.GetButtonDown("Jump") && clocks["jumpCooldown"] == 0){
            StartCoroutine(Clock("jumpBuffer", player.jumpForgiveness));
            StartCoroutine(Clock("frictionTimer", 2));
        }

        if (clocks["jumpBuffer"] <= player.jumpForgiveness && clocks["jumpBuffer"] != 0 && clocks["jumpCooldown"] == 0){
            Jump(this, player, velocity, isOnGround, hit);
        }


        // air
        if (!isOnGround){
            AirControl(this, player, velocity, accelXZ);
            Gravity(this, player, velocity, hit);
        }

        // - CROUCH: 0 is uncrouched, 1 is failing to stand, 2 is crouching manually
        if (Input.GetButton("Crouch")){
            cc.center = new Vector3(0, 0.5F, 0);
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
        if (Input.GetButtonDown("Crouch") && velocity.magnitude > player.walkSpeed * 0.9F && clocks["slideBuffer"] == 0 && isOnGround && heat < 1){
            StartCoroutine(Clock("slideBuffer", player.jumpForgiveness));
        }
        else if (Input.GetButtonDown("Crouch") && heat >= 1) {
            Debug.Log("Too hot!");
        }

        if (clocks["slideBuffer"] <= player.jumpForgiveness && clocks["slideBuffer"] != 0 && clocks["slide"] == 0){

            GameEvents.current.playerSlide(this);
            GameEvents.current.HeatPlayer(this, "Slide");
            StartCoroutine(Clock("slide", player.slideCooldown));
            Slide();
        }

        if (clocks["slide"] !=0){
            Slide();
        }

        //ground magnetism - will push you down perpendicular to a standable surface when youre very close to it to keep you from ramping off tiny things
        if (hit.distance <= cc.height / 2 + player.gMagThreshold && clocks["jumpCooldown"] == 0 && slopeState == 0){
            velocity -= hit.normal * player.gMagnetism;
        }

        if (!isOnGround){
            // we know we're on the ground if we get past here because of this guard statement
            return;
        }

        if (slideState == 1){
            //we know we're not in the middle of a slide past here because of this guard statement
            return;
        }

        if (accelXZ.magnitude > 0.01F){
            Walk(this, player, velocity, accelXZ, hit, walkSpeedAdj);
        }
        else {
            Friction(this, player, velocity);
        }
    }
    
    public void StartJumpCooldown(){
        if (clocks["jumpCooldown"] == 0){
            StartCoroutine(Clock("jumpCooldown", player.coyoteTime));
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
            cc.center = new Vector3(0, 0, 0);
            cc.height = player.height;

            Crouch(this, player, crouchState, isOnGround, hit);
            crouchState = 0;
        }
    }

    void Slide(){ //i know its weird that slide doesn't have its own script but it'd be a hassle to try and manage the timers in another script and it isnt that big so whatever
        Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
        Vector3 slideVector;

        if (accelXZ.magnitude > 0.1F){
            slideVector = accelXZ.normalized;
        }
        else {
            slideVector = transform.forward;
        }

        if (clocks["jumpCooldown"] != 0){ //slide jump
            GameEvents.current.playerSlideInterrupted(this);
            slideState = 0;

            return;
        }

        if (clocks["slide"] > player.slideITicks){ //end of slide i frames
            slideState = 0;

            return;
        }

        //we know slide will be executed after this point
        slideState = 1;
        if (velocityXZ.magnitude < player.slideForce){
            velocity = slideVector * player.slideForce;
        }
        else{
            velocity = slideVector * velocityXZ.magnitude;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(cc.radius * 2, cc.height, cc.radius * 2));
    }

    bool unsloped = false;
    bool surfed = false;
    bool slipped = false;
    void SlopeTest(){
        if (hit.distance <= cc.height / 2 + player.gMagThreshold && Vector3.Dot(hit.normal, transform.up) < player.maxSlope && velocity.magnitude > player.walkSpeed){
            /*player is surfing if they are within the ground check distance but the ground is too steep to trigger the ground check, and they are traveling faster than walking speed
            vertical speed also counts more towards surfing, so players won't start to slip as easy if theyre surfing right up an incline because thats fun*/
            slopeState = 2;
            unsloped = false;
            Slope(this, player, velocity, slopeState, hit);
            if (!surfed){
                GameEvents.current.playerSurfEnter(this);
                surfed = true;
                slipped = false;
            }
        }
        else if (hit.distance <= cc.height / 2 + player.gMagThreshold + 0.5F && Vector3.Dot(hit.normal, transform.up) < player.maxSlope){
            slopeState = 1;
            unsloped = false;
            Slope(this, player, velocity, slopeState, hit);

            if (surfed && !slipped){
                GameEvents.current.playerSlipEnter(this);
                surfed = false;
                slipped = true;
            }
        }
        else {
            slopeState = 0;
            if (!unsloped){
                //calls the GameEvents system to invoke the Unslope event only once
                unsloped = true;
                slipped = false;
                GameEvents.current.playerUnslope(this);
            }
            if (surfed){
                surfed = false;
                GameEvents.current.playerUnsurf(this);
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
        //adds clock if it doesn't exist
        try {
            clocks.Add(id, 0);
        }
        catch {
        }
        int i = 0;
        while (i <= interval){
            i++;
            clocks[id] = i;
            if (id == "slide" ){ Debug.Log(id + ": " + i); }

            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        i = 0;
        clocks[id] = i;
    }
}
