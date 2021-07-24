using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using RootMotion.FinalIK;

[SelectionBase]
public class Character : MonoBehaviour
{
  public Transform testPosition;
    [Header("General Variables")]
    [SerializeField]
    [Tooltip("General mouse sensitivity")]
    private float mouseSensitivity = 1.0f;
    public float MouseSensitivity { get { return mouseSensitivity; } set { mouseSensitivity = value; } }
    [SerializeField]
    [Tooltip("Ratio of camera height compared to the character collider")]
    [Range(0,1)]
    private float cameraHeightRatio = .9f;
    [Header("Running Variables")]
    [SerializeField]
    [Tooltip("Max Speed of Player Running")]
    [Range(5,30)]
    private float maxRunSpeed = 1.0f;
    public float MaxRunSpeed { get { return maxRunSpeed; } set { maxRunSpeed = value; } }
    [SerializeField]
    [Tooltip("How fast the Player speeds up")]
    [Range(5,30)]
    private float runAccSpeed = 1.0f;
    public float RunAccSpeed { get { return runAccSpeed; } set { runAccSpeed = value; } }
    [SerializeField]
    [Tooltip("How fast the Player slows down")]
    [Range(5,30)]
    private float runDecSpeed = 1.0f;
    public float RunDecSpeed { get { return runDecSpeed; } set { runDecSpeed = value; } }
    [SerializeField]
    [Tooltip("Force of Player Jump")]
    [Range(5,30)]
    private float jumpForce = 1.0f;
    public float JumpForce { get { return jumpForce; } set { jumpForce = value; } }
    public Animator animator;
    [Header("Sliding Variables")]
    [SerializeField]
    [Tooltip("Max speed when crouching")]
    [Range(5,30)]
    private float maxCrouchSpeed = 1.0f;
    public float MaxCrouchSpeed { get { return maxCrouchSpeed; } set { maxCrouchSpeed = value; } }
    [SerializeField]
    [Tooltip("How fast the player accelerates to crouching speed")]
    [Range(5,30)]
    private float crouchAccSpeed = 1.0f;
    public float CrouchAccSpeed { get { return crouchAccSpeed; } set { crouchAccSpeed = value; } }
    [SerializeField]
    [Tooltip("How fast the collider / camera transition to crouching height")]
    [Range(5,30)]
    private float crouchSharpness = 10f;
    public float CrouchSharpness { get { return crouchSharpness; } set { crouchSharpness = value; } }
    [SerializeField]
    [Tooltip("Maximum speed when sliding")]
    [Range(5,30)]
    private float maxSlideSpeed = 1.0f;
    public float MaxSlideSpeed { get { return maxSlideSpeed; } set { maxSlideSpeed = value; } }
    [SerializeField]
    private float minSlideSpeed = 1.0f;
    public float MinSlideSpeed { get { return minSlideSpeed; } set { minSlideSpeed = value; } }
    [SerializeField]
    private float slideAccSpeed = 1.0f;
    public float SlideAccSpeed { get { return slideAccSpeed; } set { slideAccSpeed = value; } }
    [Header("Air Movement Variables")]
    [SerializeField]
    private float gravity = 1.0f;
    public float Gravity { get { return gravity; } set { gravity = value; } }
    [SerializeField]
    private float maxAirSpeed = 1.0f;
    public float MaxAirSpeed { get { return maxAirSpeed; } set { maxAirSpeed = value; } }
    [SerializeField]
    private float airAcc = 1.0f;
    public float AirAcc { get { return airAcc; } set { airAcc = value; } }
    [Header("Wall Run Variables")]
    [SerializeField]
    private float wallSpeedMod = 1.0f;
    public float WallSpeedMod { get { return wallSpeedMod; } set { wallSpeedMod = value; } }
    [SerializeField]
    private float maxBackToWallSpeed = 1.0f;
    public float MaxBackToWallSpeed { get { return maxBackToWallSpeed; } set { maxBackToWallSpeed = value; } }
    [SerializeField]
    private float wallRunGravity = 1.0f;
    public float WallRunGravity { get { return wallRunGravity; } set { wallRunGravity = value; } }
    [SerializeField]
    private float wallRunAccSpeed = 1.0f;
    public float WallRunAccSpeed { get { return wallRunAccSpeed; } set { wallRunAccSpeed = value; } }
    [SerializeField]
    private float angleRollSpeed = 1.0f;
    public float fixedAngleRollDuration = 1.0f;
    [Header("Wall Climb Variables")]
    [SerializeField]
    private float maxClimbSpeed = 1.0f;
    public float MaxClimbSpeed { get { return maxClimbSpeed; } set { maxClimbSpeed = value; } }
    [Header("Grapple Variables")]
    [SerializeField]
    private float maxGrappleDistance;
    public float MaxGrappleDistance { get { return maxGrappleDistance; } set { maxGrappleDistance = value; } }
    [SerializeField]
    private LayerMask grappleMask;
    [SerializeField]
    private float maxGrappleSpeed;
    public float MaxGrappleSpeed { get { return maxGrappleSpeed; } set { maxGrappleSpeed = value; } }
    [SerializeField]
    private float grappleAcc;
    public float GrappleAcc { get { return grappleAcc; } set { grappleAcc = value; } }
    [SerializeField]
    private float minGrappleSpeed;
    public float MinGrappleSpeed { get { return minGrappleSpeed; } set { minGrappleSpeed = value; } }
    [SerializeField]
    private float grappleAngularAcc;
    [SerializeField]
    private float forwardMod;
    [SerializeField]
    private float sidewaysMod;
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
    [SerializeField]
    private float  grappleCooldown;
    public State running_state;
    public State falling_state;
    public State wall_running_state;
    public State sliding_state;
    public State wall_climbing_state;
    public State grappling_state;
    public StateMachine movement_machine;
    
    [Header("Other Variables")]
    public CharacterCollisions character_collisions;
    public InputHandler input_handler;
    [SerializeField]
    private LimbIK leftArm = null;
    [SerializeField]
    private LimbIK rightArm = null;

    [SerializeField]
    private Transform FPCamera = null;
    [SerializeField]
    private Transform grappleOrigin = null;
    [SerializeField]
    public GameObject tongue = null;
    [SerializeField]
    private GameObject tongueEnd = null;
    private SplineMesh.Spline tongueSpline;
    private SplineMesh.Spline tongueEndSpline;
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
    public int cur_wall_direction;
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

    public Timer GrappleCooldownTimer;
    
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
    private int grappleDirection;
    public Vector3 lastWallNormal = Vector3.zero;
    public Anim curAnimState;

    void UpdateMouseLook(){
      //get a simple vector 2 for the mouse delta 
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

      //Smoothly transition to that velocity, or slow down to 0. Use different acc modifiers 
      if(Mathf.Approximately(slope_direction.magnitude, 0f)){
        velocity = Vector3.Lerp(velocity, Vector3.zero, runDecSpeed * Time.fixedDeltaTime);
      } else { 
        velocity = Vector3.Lerp(velocity, target_velocity, runAccSpeed * Time.fixedDeltaTime);
      }
      
    }

    public void AirMovement(){
      //allow faster vectoring if the user is holding sprint, optional  
      speed_mod = input_handler.is_sprinting ? 1.3f : 1f;
      
      //Add the air acceleration to the velocity
      velocity += input_direction * airAcc * Time.fixedDeltaTime;

      //clamp sideways velocity, keep upward velocity
      float vertical_velocity = velocity.y;
      
      //project current velocity on a sideways plane
      Vector3 horizontal_velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
      
      //if the speed when you started to fall is faster than the max air speed, use the max air speed
      float max_air_velocity = Mathf.Min(last_falling_speed,maxAirSpeed);
      
      //if the max air speed was slower than the run speed, all the user to speed up still  
      max_air_velocity = Mathf.Max(max_air_velocity, maxRunSpeed);

      // clamp the velocity to that magnitude value 
      horizontal_velocity = Vector3.ClampMagnitude(horizontal_velocity, max_air_velocity);
      
      //apply horizontal and veritcal velocity
      velocity = horizontal_velocity + (Vector3.up * vertical_velocity);

      //use case for hitting ceilings, dont want to accumulate velocity 
      if(character_collisions.on_ceiling && velocity.y > 0){
        velocity.y = 0;
      }

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

      //need this for other checks, this is compared to the  inital direction 
      cur_wall_direction = Vector3.Dot(transform.TransformDirection(Vector3.forward), orthogonal_wall_vector) < 0 ? -1 : 1; 
      
      //Keep upward velocity if it positive
      if(velocity.y > 0){
        
        // save the current upward velocity
        float vertical_velocity = velocity.y;

        //project the horizontal velocity
        Vector3 horizontal_velocity = new Vector3(velocity.x, 0 ,velocity.z);
        horizontal_velocity = Vector3.ProjectOnPlane(horizontal_velocity, wall_hit_normal);

        //calculate the velocity we want horizontally
        Vector3 target_velocity = orthogonal_wall_vector * wall_direction * wallSpeedMod;

        //clamp the horizontal velocity if your back is against the wall
        if(character_collisions.wall_angle > .6){
          target_velocity = Vector3.ClampMagnitude(target_velocity, maxBackToWallSpeed);
        }

        //accelerate horizontally while keeping vertical accelearion
        velocity = Vector3.Lerp(horizontal_velocity, target_velocity, wallRunAccSpeed * Time.fixedDeltaTime) + (Vector3.up * vertical_velocity);
        
        // now apply gravity so there is a downward arc 
        velocity += Vector3.down * gravity * Time.fixedDeltaTime;
      } else {
        
        //make the velocity equal the correct direction and add the wall speed modifier   
        Vector3 target_velocity = orthogonal_wall_vector * wall_direction * wallSpeedMod;

        //clamp the horizontal velocity if your back is against the wall
        if(character_collisions.wall_angle > .6){
          target_velocity = Vector3.ClampMagnitude(target_velocity, maxBackToWallSpeed);
        }

        //accelerate to that target velocity
        velocity = Vector3.Lerp(velocity, target_velocity, wallRunAccSpeed * Time.fixedDeltaTime);
        
        // now apply gravity so there is a downward arc 
        velocity += Vector3.down * wallRunGravity * Time.fixedDeltaTime;
      }     
    }

    public void SetWallRunValues(){
      //this timer determines when wall run will end      
      wallRunDurationTimer.StartTimer();

      //get current current forward direction for character
      Vector3 along_wall = transform.TransformDirection(Vector3.forward);

      //get the wall normal, needs to be set for jumping
      wall_hit_normal = character_collisions.WallHitNormal();

      //get the orthogonal vector from the wall compared to UP 
      orthogonal_wall_vector = Vector3.Cross(wall_hit_normal, Vector3.up);
      
      //use the dot product to determine which direction the player is facing on the wall
      wall_direction = Vector3.Dot(along_wall, orthogonal_wall_vector) < 0 ? -1 : 1; 

      //snap to wall or else you get very inconsistent wall running 
      controller.Move((wall_hit_normal * -1) * (Vector3.Distance(transform.position,character_collisions.last_wall_position)- controller.radius));

      //put the character valocity along wall, needed incase velocity is too strong to keep on wall and helps with other calculations
      velocity = ((velocity.x + velocity.z) * (orthogonal_wall_vector * wall_direction).normalized) + (Vector3.up * velocity.y);
    }

    public void EnableWallRunArm(){
      if(wall_direction < 0){
        rightArm.enabled = true;
      } else {
        leftArm.enabled = true;
      }
    }

    public void DisableWallRunArms(){
      leftArm.enabled = false;
      rightArm.enabled = false;
    }

    public void EndWallRun(){
      lastWallNormal = wall_hit_normal;
    }

    public float GetWallDifference(){
      return Vector3.Dot(lastWallNormal, character_collisions.WallHitNormal());
    }
    public void WallClimb(){
      //calculate which way to climb by using the up vector projected on the wall
      Vector3 target_velocity = Vector3.ProjectOnPlane(Vector3.up, wall_hit_normal) * maxClimbSpeed;
      velocity = Vector3.Lerp(velocity, target_velocity, runAccSpeed * Time.fixedDeltaTime);
    }
    public void SetWallClimbValues(){    
      //wall climbing has a duration
      wallClimbDurationTimer.StartTimer();
      
      //get the wall normal, needs to be set for jumping
      wall_hit_normal = character_collisions.WallHitNormal();

      //get the orthogonal vector from the wall compared to UP 
      orthogonal_wall_vector = Vector3.Cross(wall_hit_normal, Vector3.up);
    }

    public void SlideMovement(){
      //this gets the current angle of slope we are on
      float current_slope = Mathf.Round(Vector3.Angle(character_collisions.ground_slope, transform.up));

      //create values used to determine the slide direction and speed 
      Vector3 slide_direction;
      float slide_speed;
      
      //this will be our end velocity and direction that we want
      Vector3 target_velocity; 
      
      // if we are on flat ground use initial values otherwise calculate slope values
      if(Mathf.Abs(current_slope) < 10){
        //this makes it so the slide valaues are equal to the initial ones
        slide_direction = last_slide_direction;
        slide_speed = last_slide_speed;

        //if you are sliding onto flat ground, start the timer
        if(!slideTimerSet){
          slideTimer.StartTimer();
          slideTimerSet = true; 
        }
        //if the timer has stopped and you are still sliding, slow down 
        if(!slideTimer.is_active){
          slide_speed = 1f;
        }
      } else {
        //if you slide from flat to sloped ground, make it so the timer will reset
        if(slideTimerSet){
          slideTimerSet = false; 
        }

        // parallel to ground
        Vector3 ground_orthogonal = Vector3.Cross(transform.up, character_collisions.ground_slope);
        
        //parallel to ground with the proper slope direction 
        Vector3 slope_orthogonal = Vector3.Cross(ground_orthogonal, character_collisions.ground_slope);
        
        //parallel to ground with current transform forward 
        slide_direction = Vector3.ProjectOnPlane(velocity.normalized + slope_orthogonal.normalized, character_collisions.ground_slope).normalized; 

        //this takes the minimum slide speed and modifies it by how steep the slope is 
        slide_speed = (1 + (current_slope / controller.slopeLimit)) * minSlideSpeed;
        
        //if that value is more than the max slide value, use the max value instead
        slide_speed = Mathf.Min(slide_speed, maxSlideSpeed);
        
        //set these variables to be used for the next calculations
        last_slide_direction = slide_direction;
        last_slide_speed = slide_speed; 
      }
      Debug.DrawRay (transform.position, slide_direction * 10, Color.green);
      
      //make simple target velocity calculation
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
      //this will give an initial burst of speed 
      last_slide_speed = velocity.magnitude * 1.3f;

      //current forward 
      last_slide_direction = transform.TransformDirection(Vector3.forward);
    }

    public bool CrouchThreshold(){
      return velocity.magnitude <= maxCrouchSpeed;
    }
    public void ResetSlideTimer(){
      slideTimerSet = false;
      slideTimer.StopTimer();
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
      velocity = Vector3.ProjectOnPlane(new Vector3(velocity.x, 0f, velocity.z), character_collisions.ground_slope);
      
      // then, add the jumpSpeed value upwards
      velocity += character_collisions.ground_slope * jumpForce;
    }

    public void GroundWallJump(){
      jumpCooldownTimer.StartTimer();
      ResetJumpBuffer();
      // start by canceling out the vertical component of our velocity
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
            tongue.SetActive(true);
            float check_dist = Vector3.Distance((tongue.transform.InverseTransformPoint(hitLocation.transform.position) + Vector3.Cross(grappleHit.normal, Vector3.up)), tongue.transform.InverseTransformPoint(transform.position));
            grappleDirection = check_dist - setGrappleDistance > 0 ? 1 : -1;
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
      
      Vector3 target_linear_velocity = grapple_dir * 3;
      //if the player is aiming within 90 degrees of grapple point
      if( grapple_angle > .7f){
        //target_sideways_velocity = forward_dir * ((1+ Mathf.Abs(1 - grapple_angle)) * maxAngularGrappleSpeed);
        target_sideways_velocity = forward_dir * maxGrappleSpeed;
        setGrappleDistance = GetGrappleDistance();
      } else {
        target_sideways_velocity = Vector3.ProjectOnPlane(forward_dir, grapple_dir).normalized * maxGrappleSpeed;
        target_linear_velocity = grapple_dir * (Mathf.Pow(target_sideways_velocity.magnitude, 2f) / setGrappleDistance);
      }
      
      Debug.DrawRay (transform.position, target_sideways_velocity, Color.green);
      
      velocity = Vector3.Lerp(velocity, target_sideways_velocity + target_linear_velocity, grappleAcc * Time.fixedDeltaTime);
    }

    public void SetTongue(){
      tongueSpline.nodes[0].Position = grappleOrigin.localPosition;
      tongueSpline.nodes[0].Direction = grappleOrigin.localPosition + (FPCamera.TransformDirection(Vector3.forward) *3 + FPCamera.TransformDirection(Vector3.down));
    
      Vector3 tongue_end = tongue.transform.InverseTransformPoint(hitLocation.transform.position);
      tongueSpline.nodes[1].Position =  tongue_end;
      Vector3 scale = new Vector3(.2f, .2f, .2f);
      tongueSpline.nodes[1].Direction = tongue_end + (Vector3.Cross(grappleHit.normal, Vector3.up) *grappleDirection); //  (Vector3.Cross(grappleHit.normal, Vector3.up)* 3);//Vector3.Scale(grappleHit.normal * -1, scale);

      // tongueEndSpline.nodes[0].Position = tongue.transform.InverseTransformPoint(hitLocation.transform.position) +  (Vector3.Cross(grappleHit.normal, Vector3.up)* .3f);
      // tongueEndSpline.nodes[0].Direction = tongue.transform.InverseTransformPoint(hitLocation.transform.position)+  (Vector3.Cross(grappleHit.normal, Vector3.up)* .3f);
      //tongueEndSpline.nodes[1].Position = grappleOrigin.localPosition;
      //tongueEndSpline.nodes[1].Direction = grappleOrigin.localPosition + (FPCamera.TransformDirection(Vector3.forward) *3);
      // tongueEndSpline.nodes[1].Position = tongue.transform.InverseTransformPoint(hitLocation.transform.position) -  (Vector3.Cross(grappleHit.normal, Vector3.up)* .7f);
      // tongueEndSpline.nodes[1].Direction = tongue.transform.InverseTransformPoint(hitLocation.transform.position) -  (Vector3.Cross(grappleHit.normal, Vector3.up)* .7f);
      //tongueEndSpline.nodes[0].Position = tongue_end;
      //tongueEndSpline.nodes[0].Direction = tongue_end +  (Vector3.Cross(grappleHit.normal, Vector3.up)* 3);
    }

    public float GetGrappleDistance(){
      return Vector3.Distance(hitLocation.transform.position, transform.position);
    }

    public float GetGrappleAngle(){
      Vector3 forward_dir = FPCamera.TransformDirection(Vector3.forward);
      //get current vector towards grapple point
      Vector3 grapple_dir = (hitLocation.transform.position - transform.position).normalized;
      
      //get dot product to represent how close to grapple you are aiming
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
      if( input_handler.is_jumping){//Keyboard.current.spaceKey.wasPressedThisFrame){
        if(jumpBufferTimer.is_active){
          jumpBufferTimer.StopTimer();
          jumpBufferTimer.StartTimer();
          input_handler.is_jumping = false;
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
        if(movement_machine.cur_state != grappling_state && !GrappleCooldownTimer.is_active){
          stopGrapple = false;
          movement_machine.ChangeState(grappling_state);
        }
    }
    void OnGrappleUp(){
        if(movement_machine.cur_state == grappling_state){
          stopGrapple = true;
          GrappleCooldownTimer.StartTimer();
        }
    }

    public void SetAnimation(Anim index){
      animator.SetInteger("Change", ((int)index));
      curAnimState = index;
    }

    public void SetSenitivity(float val){
      mouseSensitivity = val;
    }
    
    private void Awake() {
      Application.targetFrameRate = 120;
    }
    void Start()
    {
      controller = GetComponent<CharacterController>();
      character_collisions = GetComponent<CharacterCollisions>();
      input_handler = GetComponent<InputHandler>();
      tongueSpline = tongue.GetComponent<SplineMesh.Spline>();
      tongueEndSpline = tongueEnd.GetComponent<SplineMesh.Spline>();

      SetAnimation(((int)Anim.Idle));
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
      GrappleCooldownTimer = gameObject.AddComponent<Timer>();
      GrappleCooldownTimer.SetTimer(grappleCooldown);
      
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
      if(!jumpCooldownTimer.is_active && !wallJumpCooldownTimer.is_active){
        can_jump = JumpBuffer();
      }
      if(input_handler.is_grappling){
        if(movement_machine.cur_state != grappling_state){
          movement_machine.ChangeState(grappling_state);
        }
      }
      movement_machine.cur_state.HandleInput();
      movement_machine.cur_state.LogicUpdate();
    }

    void FixedUpdate() {

      // tongueSpline.nodes[0].Position = grappleOrigin.localPosition;
      // tongueSpline.nodes[0].Direction = grappleOrigin.localPosition + (FPCamera.TransformDirection(Vector3.forward) *5);
    
      // Vector3 tongue_end = tongue.transform.InverseTransformPoint(testPosition.position);
      // tongueSpline.nodes[1].Position =  tongue_end;
      // Vector3 scale = new Vector3(.2f, .2f, .2f);
      // tongueSpline.nodes[1].Direction = tongue_end +  (Vector3.Cross(testPosition.TransformDirection(Vector3.forward), Vector3.up)* 5);//Vector3.Scale(grappleHit.normal * -1, scale);
      //input_direction = input_handler.move_input;
      input_direction = transform.right * input_handler.move_input.x + transform.forward * input_handler.move_input.z;
      movement_machine.cur_state.PhysicsUpdate();
      controller.Move(velocity * Time.fixedDeltaTime);
      if(debug_speed != null){
        debug_speed.text =  ((int)(((new Vector3(velocity.x, 0 , velocity.z).magnitude / 1000) * 60) * 60)).ToString();
      }
      //print(Vector3.Dot(input_direction, character_collisions.last_wall_normal) + " | " +input_direction + " | " +  character_collisions.last_wall_normal);
    }
}

public enum Anim : int{Idle, Running, Sliding, Falling, Climbing}
