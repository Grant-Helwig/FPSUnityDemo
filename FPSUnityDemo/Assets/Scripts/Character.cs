using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class Character : MonoBehaviour
{
    [Header("General Variables")]
    [SerializeField]
    [Tooltip("General mouse sensitivity")]
    private float mouseSensitivity = 1.0f;
    [SerializeField]
    [Tooltip("Ratio of camera height compared to the character collider")]
    [Range(0,1)]
    private float cameraHeightRatio = .9f;
    [Header("Running Variables")]
    [SerializeField]
    private float maxRunSpeed = 1.0f;
    [SerializeField]
    private float runAccSpeed = 1.0f;
    [SerializeField]
    private float jumpForce = 1.0f;
    [Header("Sliding Variables")]
    [SerializeField]
    private float maxCrouchSpeed = 1.0f;
    [SerializeField]
    private float crouchAccSpeed = 1.0f;
    [SerializeField]
    private float crouchSharpness = 10f;
    [SerializeField]
    private float maxSlideSpeed = 1.0f;
    [SerializeField]
    private float minSlideSpeed = 1.0f;
    [SerializeField]
    private float slideAccSpeed = 1.0f;
    
    [Header("Air Movement Variables")]
    [SerializeField]
    private float gravity = 1.0f;
    [SerializeField]
    private float maxAirSpeed = 1.0f;
    [SerializeField]
    private float airAcc = 1.0f;
    
    [Header("Wall Movement Variables")]
    [SerializeField]
    private float wallSpeedMod = 1.0f;
    [SerializeField]
    private float wallRunGravity = 1.0f;
    
    [SerializeField]
    private float maxClimbSpeed = 1.0f;
    [SerializeField]
    private float angleRollSpeed = 1.0f;
    public float fixedAngleRollDuration = 1.0f;
    [Header("Timer Variables")]
    [SerializeField]
    private float slideTime;
    [SerializeField]
    private float jumpCooldownTime;
    [SerializeField]
    private float jumpBufferTime;
    [SerializeField]
    private float coyoteTime;
    [SerializeField]
    private float  wallRunDuration;
    [SerializeField]
    private float  wallClimbDuration;
    public State running_state;
    public State falling_state;
    public State wall_running_state;
    public State sliding_state;
    public State wall_climbing_state;
    public State grappling_state;
    public StateMachine movement_machine;
    [Header("Grapple Variables")]
    [SerializeField]
    private float maxGrappleDistance;
    [SerializeField]
    private LayerMask grappleMask;
    [SerializeField]
    private float maxGrappleSpeed;
    [SerializeField]
    private float grappleAcc;
    [SerializeField]
    private float minGrappleSpeed;
    [SerializeField]
    private float grappleAngularAcc;
    [SerializeField]
    private float forwardMod;
    [SerializeField]
    private float sidewaysMod;
    [Header("Other Variables")]
    public CharacterCollisions character_collisions;
    public InputHandler input_handler;

    [SerializeField]
    private Transform FPCamera = null;
    [SerializeField]
    private Transform lookAt;
    [SerializeField]
    private Transform mouth;
    private GameObject hitLocation;
    private RaycastHit cameraHit;
    private RaycastHit grappleHit;

    public float max_angle_roll = 1.0f;
    
    [SerializeField]
    public CharacterController controller = null;
    public bool can_wall_run = true;
    
    public bool lock_cursor;
    
    private float camera_pitch = 0;

    public Vector3 velocity;
    public Vector3 input_direction;
    private Vector3 wall_hit_normal;
    private Vector3 orthogonal_wall_vector;
    public int wall_direction;
    private float speed_mod = 1.0f;
    private float last_slide_speed;
    private Vector3 last_slide_direction;
    public Timer slideTimer;
    
    public Timer jumpCooldownTimer;
    public Timer wallJumpCooldownTimer;
    
    public Timer jumpBufferTimer;
    
    public Timer coyoteTimer; 
    
    public Timer wallRunDurationTimer; 
    
    public Timer wallClimbDurationTimer; 
    
    public float standing_height;
    public float crouching_height;
    private bool slideTimerSet = false;
    public bool can_jump;
    private float last_falling_speed;
    public float current_camera_roll;
    [SerializeField]
    public Text debug_text;
    [SerializeField]
    public Text debug_speed;
    public bool stopGrapple = false;
    private float setGrappleDistance = 0f;
    void UpdateMouseLook(){
      //get a simple vector 2 for the mouse delta 
      //Vector2 mouse_delta = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
      Vector2 mouse_delta = input_handler.mouse_delta;

      //set the y rotation for the camera, user the mouse sensitivity 
      camera_pitch -= mouse_delta.y * mouseSensitivity;

      //clamp the rotation
      camera_pitch = Mathf.Clamp(camera_pitch,-90, 90);

      //set the camera rotation
      FPCamera.localEulerAngles = new Vector3(camera_pitch, 0, current_camera_roll); 

      //apply this rotation with the sensitivity modifier 
      transform.Rotate(Vector3.up * mouse_delta.x * mouseSensitivity);
    }

    public void GroundMovement(){
      //sprint modifier applied 
      speed_mod = input_handler.is_sprinting ? 1.3f : 1f;
      
      //take slop into account
      Vector3 slope_direction = Vector3.ProjectOnPlane(input_direction, character_collisions.ground_slope);
      //Get the speed you want to get to 
      Vector3 target_velocity = slope_direction * maxRunSpeed * speed_mod;
      //Smoothly transition to that velocity 
      velocity = Vector3.Lerp(velocity, target_velocity, runAccSpeed * Time.fixedDeltaTime);
    }

    public void AirMovement(){
      speed_mod = input_handler.is_sprinting ? 1.3f : 1f;
      
      //Add the air acceleration to the velocity
      velocity += input_direction * airAcc * Time.fixedDeltaTime;

      //clamp sideways velocity, keep upward velocity
      float vertical_velocity = velocity.y;
      Vector3 horizontal_velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
      float max_air_velocity = Mathf.Min(last_falling_speed,maxAirSpeed);
      max_air_velocity = Mathf.Max(max_air_velocity, maxRunSpeed);
      horizontal_velocity = Vector3.ClampMagnitude(horizontal_velocity, max_air_velocity);
      velocity = horizontal_velocity + (Vector3.up * vertical_velocity);

      // now add gravity acceleration
      velocity += Vector3.down * gravity * Time.fixedDeltaTime;
    }
    public void SetAirValues(float speed){
      last_falling_speed = speed;
    }

    public void WallRun(){
      //get the wall normal, needs to be set for jumping
      wall_hit_normal = character_collisions.WallHitNormal();
      //get the orthogonal vector from the wall compared to UP 
      orthogonal_wall_vector = Vector3.Cross(wall_hit_normal, Vector3.up);

      if(velocity.y > 0){
        //Keep upward velocity if it positive
        float vertical_velocity = velocity.y;
        Vector3 horizontal_velocity = Vector3.ProjectOnPlane(orthogonal_wall_vector * wall_direction * wallSpeedMod, Vector3.up);
        velocity = horizontal_velocity + (Vector3.up * vertical_velocity);
      } else{
        //make the velocity equal the correct direction and add the wall speed modifier 
        velocity = orthogonal_wall_vector * wall_direction * wallSpeedMod;
      }     

      // now apply gravity so there is a downward arc 
      velocity += Vector3.down * wallRunGravity * Time.fixedDeltaTime;
    }

    public void WallClimb(){
      //Vector3 target_velocity = Vector3.ProjectOnPlane(Vector3.up, wall_hit_normal) * maxClimbSpeed;
      Vector3 target_velocity =  Vector3.up * maxClimbSpeed;
      velocity = Vector3.Lerp(velocity, target_velocity, runAccSpeed * Time.fixedDeltaTime);
    }

    public void SlideMovement(){
      //this gets the current angle of slope we are on
      float current_slope = Mathf.Round(Vector3.Angle(character_collisions.ground_slope, transform.up));
      
      //create values used to determine the slide direction and speed 
      Vector3 slide_direction;
      float slide_speed;
      Vector3 target_velocity; 
      // if we are on flat ground use initial values otherwise calculate slope values
      if(Mathf.Approximately(current_slope, 0)){
        slide_direction = last_slide_direction;
        slide_speed = last_slide_speed;
        if(!slideTimerSet){
          slideTimer.StartTimer();
          slideTimerSet = true; 
        }
        if(!slideTimer.is_active){
          slide_speed = 1f;
        }
      } else {
        if(slideTimerSet){
          slideTimerSet = false; 
        }
        // parallel to ground
        Vector3 ground_orthogonal = Vector3.Cross(transform.up, character_collisions.ground_slope);
        //parallel to ground with the proper slope direction 
        Vector3 slope_orthogonal = Vector3.Cross(ground_orthogonal, character_collisions.ground_slope);
        //parallel to ground with current transform forward 
        slide_direction = Vector3.ProjectOnPlane(velocity.normalized + slope_orthogonal.normalized, character_collisions.ground_slope).normalized; 

        slide_speed = (1 + (current_slope / controller.slopeLimit)) * minSlideSpeed;
        slide_speed = Mathf.Min(slide_speed, maxSlideSpeed);
        //slide_speed =  (1f - character_collisions.ground_slope.y) * );
        last_slide_direction = slide_direction;
        last_slide_speed = slide_speed; 
      }
      Debug.DrawRay (transform.position, slide_direction * 10, Color.green);
      
      target_velocity = slide_direction * slide_speed;

      
      // perform a crouch if your current velocity is less than half your run speed along with your target velocity 
      if(velocity.magnitude <= maxCrouchSpeed 
      && target_velocity.magnitude <= maxCrouchSpeed
      && !slideTimer.is_active){
        //take slope into account
        Vector3 slope_direction = Vector3.ProjectOnPlane(input_direction, character_collisions.ground_slope);
        //Get the speed you want to get to 
        Vector3 crouch_velocity = slope_direction * maxCrouchSpeed * speed_mod;
        //Smoothly transition to that velocity 
        velocity = Vector3.Lerp(velocity, crouch_velocity, crouchAccSpeed * Time.fixedDeltaTime);
      }  else {
        velocity = Vector3.Lerp(velocity, target_velocity, slideAccSpeed * Time.fixedDeltaTime);
      }
    }

    public void SetSlideValues(){
      last_slide_speed = velocity.magnitude * 1.3f;
      last_slide_direction = transform.TransformDirection(Vector3.forward);
    }

    public bool CrouchThreshold(){
      return velocity.magnitude <= maxCrouchSpeed;
    }
    public void ResetSlideTimer(){
      slideTimerSet = false;
      slideTimer.StopTimer();
    }

    public void SetWallRunValues(){      
      wallRunDurationTimer.StartTimer();
      //get current current forward direction for character
      Vector3 along_wall = transform.TransformDirection(Vector3.forward);

      //get the wall normal, needs to be set for jumping
      wall_hit_normal = character_collisions.WallHitNormal();

      //get the orthogonal vector from the wall compared to UP 
      orthogonal_wall_vector = Vector3.Cross(wall_hit_normal, Vector3.up);
      
      //use the dot product to determine which direction the player is facing on the wall
      wall_direction = Vector3.Dot(along_wall, orthogonal_wall_vector) < 0 ? -1 : 1; 
    }

    public void SetWallClimbValues(){    
      wallClimbDurationTimer.StartTimer();
      //get the wall normal, needs to be set for jumping
      wall_hit_normal = character_collisions.WallHitNormal();

      //get the orthogonal vector from the wall compared to UP 
      orthogonal_wall_vector = Vector3.Cross(wall_hit_normal, Vector3.up);
    }

    public void WallJump(){
      wallJumpCooldownTimer.StartTimer();
      ResetJumpBuffer();
      // start by canceling out the vertical component of our velocity
      velocity = new Vector3(velocity.x, 0f, velocity.z);
      
      // then, add the jumpSpeed value in the wall direction
      velocity += (wall_hit_normal + Vector3.up) * jumpForce;
    }
    public void GroundJump(){
      jumpCooldownTimer.StartTimer();
      ResetJumpBuffer();
      // start by canceling out the vertical component of our velocity
      //velocity = new Vector3(velocity.x, 0f, velocity.z);
      velocity = Vector3.ProjectOnPlane(new Vector3(velocity.x, 0f, velocity.z), character_collisions.ground_slope);
      // then, add the jumpSpeed value upwards
      velocity += character_collisions.ground_slope * jumpForce;
    }

    public void GroundWallJump(){
      jumpCooldownTimer.StartTimer();
      ResetJumpBuffer();
      // start by canceling out the vertical component of our velocity
      //velocity = new Vector3(velocity.x, 0f, velocity.z);
      velocity = Vector3.ProjectOnPlane(new Vector3(velocity.x, 0f, velocity.z), Vector3.Cross(character_collisions.WallHitNormal(), Vector3.up));
      // then, add the jumpSpeed value upwards
      velocity += character_collisions.ground_slope * jumpForce;
    }
    public bool StartGrapple(){
        if(Physics.Raycast(lookAt.position, FPCamera.forward, out cameraHit, maxGrappleDistance, grappleMask) &&
          Physics.Raycast(mouth.position, (cameraHit.point- mouth.position).normalized, out grappleHit, grappleMask, grappleMask)) {
            hitLocation = new GameObject();
            hitLocation.transform.position = grappleHit.point;
            hitLocation.transform.parent = grappleHit.transform;
            setGrappleDistance = GetGrappleDistance();
            return true;
        } else {
            return false;
        }
    }

    public void GrappleMovement(){
      //get current aiming forward vector
      Vector3 forward_dir = FPCamera.TransformDirection(Vector3.forward);
      //get current vector towards grapple point
      Vector3 grapple_dir = (hitLocation.transform.position - transform.position).normalized;
      
      //get dot product to represent how close to grapple you are aiming
      float grapple_angle = Vector3.Dot(forward_dir, grapple_dir);
      if(grapple_angle < 0){
        grapple_angle = 0f;
      }

      //variable used for sidways velocities
      Vector3 target_sideways_velocity;

      //these variables will blend the fast speed betwen the forward and the sideways vectors depending on grapple angle
      float linear_speed = minGrappleSpeed + ((maxGrappleSpeed - minGrappleSpeed) * grapple_angle);
      float sideways_speed = minGrappleSpeed + ((maxGrappleSpeed - minGrappleSpeed) * (1 - grapple_angle));
      
      Vector3 target_linear_velocity = Vector3.zero;
      //if the player is aiming within 90 degrees of grapple point
      if(grapple_angle > .3f){
        //target_sideways_velocity = forward_dir * ((1+ Mathf.Abs(1 - grapple_angle)) * maxAngularGrappleSpeed);
        target_sideways_velocity = forward_dir * maxGrappleSpeed;
      } else {
        target_sideways_velocity = Vector3.ProjectOnPlane(forward_dir, grapple_dir).normalized * maxGrappleSpeed;
        target_linear_velocity = grapple_dir * (Mathf.Pow(target_sideways_velocity.magnitude, 2f) / setGrappleDistance);
      }
      
      Debug.DrawRay (transform.position, target_sideways_velocity, Color.green);
      //print("Linear Speed: " + linear_speed + "Sideways Speed: " + sideways_speed);
      print(GetGrappleDistance());
      velocity = Vector3.Lerp(velocity, target_sideways_velocity + target_linear_velocity, grappleAcc * Time.fixedDeltaTime);
    }

    public float GetGrappleDistance(){
      return Vector3.Distance(hitLocation.transform.position, transform.position);
    }

    public float GetGrappleAngle(){
      Vector3 forward_dir = FPCamera.TransformDirection(Vector3.forward);
      //get current vector towards grapple point
      Vector3 grapple_dir = (hitLocation.transform.position - transform.position).normalized;
      
      //get dot product to represent how close to grapple you are aiming
      //print(Vector3.Dot(forward_dir, grapple_dir));
      return Vector3.Dot(forward_dir, grapple_dir);
    }

    public void SetCharacterHeight(bool force, float height){
      if(force){
        controller.height = height;
        controller.center = Vector3.up * height * .5f;
        FPCamera.transform.localPosition = Vector3.up * height * cameraHeightRatio;
      } else {
        controller.height = Mathf.Lerp(controller.height, height, crouchSharpness * Time.fixedDeltaTime);
        controller.center = Vector3.up * height * .5f;
        FPCamera.transform.localPosition = Vector3.Lerp(FPCamera.transform.localPosition, Vector3.up * height * cameraHeightRatio, crouchSharpness * Time.fixedDeltaTime);
      }
    }

    public float SetCameraAngle(float target_angle){
      float camera_angle = FPCamera.eulerAngles.z;

      current_camera_roll = Mathf.LerpAngle(camera_angle,target_angle, angleRollSpeed * Time.fixedDeltaTime);
      return FPCamera.eulerAngles.z;
    }

    public void SetCameraAngleFixed(float target_angle, float start_angle, float duration){

      current_camera_roll = Mathf.LerpAngle(start_angle,target_angle, duration);
    }

    public bool AtHeight(float height){
      return controller.height == height;
    }
    

    private bool JumpBuffer(){
      if(Keyboard.current.spaceKey.wasPressedThisFrame){
        if(jumpBufferTimer.is_active){
          jumpBufferTimer.StopTimer();
          jumpBufferTimer.StartTimer();
        } else {
          jumpBufferTimer.StartTimer();
        }
      }

      return (jumpBufferTimer.is_active );
    }
    private void ResetJumpBuffer(){
      jumpBufferTimer.StopTimer();
      can_jump = false;
    }

    public void SetDebugText(string t){
      if(debug_text != null){
        debug_text.text = t;
      }
    }
    public void SnapToGround(){
      if(character_collisions.ground_hit.distance > controller.skinWidth){
        controller.Move(Vector3.down * character_collisions.ground_hit.distance);
      }
    }

    private void OnGrapple(){

    }
    
    void OnGrappleDown(){
        print("grappled");
        if(movement_machine.cur_state != grappling_state){
          stopGrapple = false;
          movement_machine.ChangeState(grappling_state);
        }
    }
    void OnGrappleUp(){
        print("stop grapple");
        if(movement_machine.cur_state == grappling_state){
          stopGrapple = true;
        }
    }
    void Start()
    {
      controller = GetComponent<CharacterController>();
      character_collisions = GetComponent<CharacterCollisions>();
      input_handler = GetComponent<InputHandler>();
      standing_height = controller.height;
      crouching_height = standing_height / 2;

      // initialize all state machine variables
      movement_machine = new StateMachine();
      running_state = new RunningState(this, movement_machine);
      falling_state = new FallingState(this, movement_machine);
      sliding_state = new SlidingState(this, movement_machine);
      wall_climbing_state = new WallClimbingState(this, movement_machine);
      wall_running_state = new WallRunningState(this, movement_machine);
      grappling_state = new GrapplingState(this, movement_machine);

      //initialize timers to set values
      slideTimer = gameObject.AddComponent<Timer>();
      slideTimer.SetTimer(slideTime);
      jumpBufferTimer = gameObject.AddComponent<Timer>();
      jumpBufferTimer.SetTimer(jumpBufferTime);
      jumpCooldownTimer = gameObject.AddComponent<Timer>();
      jumpCooldownTimer.SetTimer(jumpCooldownTime);
      wallJumpCooldownTimer = gameObject.AddComponent<Timer>();
      wallJumpCooldownTimer.SetTimer(jumpCooldownTime);
      coyoteTimer = gameObject.AddComponent<Timer>();
      coyoteTimer.SetTimer(coyoteTime);
      wallRunDurationTimer = gameObject.AddComponent<Timer>();
      wallRunDurationTimer.SetTimer(wallRunDuration);
      wallClimbDurationTimer = gameObject.AddComponent<Timer>();
      wallClimbDurationTimer.SetTimer(wallClimbDuration);
      
      //default to the failling state 
      movement_machine.Initialize(falling_state);

      //lock cursor to screen and hide cursor
      if(lock_cursor){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
      }
    }

    void Update()
    {
      UpdateMouseLook();
      can_jump = JumpBuffer();
      if(input_handler.is_grappling){
        if(movement_machine.cur_state != grappling_state){
          movement_machine.ChangeState(grappling_state);
        }
      }
      movement_machine.cur_state.HandleInput();
      movement_machine.cur_state.LogicUpdate();
    }

    void FixedUpdate() {
      
      //input_direction = input_handler.move_input;
      input_direction = transform.right * input_handler.move_input.x + transform.forward * input_handler.move_input.z;
      movement_machine.cur_state.PhysicsUpdate();
      controller.Move(velocity * Time.fixedDeltaTime);
      if(debug_speed != null){
        debug_speed.text =  ((int)(((new Vector3(velocity.x, 0 , velocity.z).magnitude / 1000) * 60) * 60)).ToString();
      }
    }
}
