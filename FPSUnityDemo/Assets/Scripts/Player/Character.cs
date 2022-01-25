using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using RootMotion.FinalIK;
using Unity.Netcode;
using Unity.Netcode.Samples;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;
using System;

[SelectionBase]
public class Character : NetworkBehaviour
{
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
    public Animator animatorThirdPerson;
    public GameObject objectThirdPerson;
    public BipedIK thirdPersonIK;
    public GrounderBipedIK grounderBipedIK;
    public GameObject hipIK;
    public Transform standingHips;
    public Transform crouchingHips;
    public Transform thirdPerson;
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
    [Tooltip("Speed floor when sliding")]
    [Range(5,30)]
    private float minSlideSpeed = 1.0f;
    public float MinSlideSpeed { get { return minSlideSpeed; } set { minSlideSpeed = value; } }
    [SerializeField]
    [Tooltip("Slide acceleration")]
    [Range(5,30)]
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
    public State mantle_state;
    public State ragdoll_state;
    public StateMachine movement_machine;
    
    public State locked_state;
    public State grapple_state;
    public State idle_state;
    public State reload_state;
    public State shoot_state;

    public float restingSize;
    public float maxSize;
    private float currentSize = 100f;
    public float aimSpeed;
    public RectTransform reticle;

    private NetworkVariable<Anim> netowrkAnimationState;

    public StateMachine action_machine;
    [Header("Weapon Variables")]
    [SerializeField]
    [Tooltip("Sway Amount")]
    [Range(0,20)]
    private float swayAmount = 1.0f;
    public float SwayAmount { get { return swayAmount; } set { swayAmount = value; } }
    [SerializeField]
    [Tooltip("Sway Smoothing")]
    [Range(0,1)]
    private float swaySmoothing = 1.0f;
    public float SwaySmoothing { get { return swaySmoothing; } set { swaySmoothing = value; } }
    [SerializeField]
    [Tooltip("Sway  Reset Smoothing")]
    [Range(0,1)]
    private float swayResetSmoothing = 1.0f;
    public float SwayResetSmoothing { get { return swayResetSmoothing; } set { swayResetSmoothing = value; } }
    [SerializeField]
    [Tooltip("Sway Clamp X")]
    [Range(0,10)]
    private float swayClampX = 1.0f;
    public float SwayClampX { get { return swayClampX; } set { swayClampX = value; } }
    [SerializeField]
    [Tooltip("Sway Clamp Y")]
    [Range(0,10)]
    private float swayClampY = 1.0f;
    public float SwayClampY { get { return swayClampY; } set { swayClampY = value; } }
    [SerializeField]
    [Tooltip("movement Sway Amount")]
    [Range(0,20)]
    private float swayMovementX = 1.0f;
    public float SwayMovementX { get { return swayMovementX; } set { swayMovementX = value; } }
    [SerializeField]
    [Tooltip("movement Sway Amount")]
    [Range(0,20)]
    private float swayMovementY = 1.0f;
    public float SwayMovementY { get { return swayMovementY; } set { swayMovementY = value; } }
    [SerializeField]
    [Tooltip("movement Sway Smoothing Amount")]
    [Range(0,20)]
    private float swayMovementSmoothing = 1.0f;
    public float SwayMovementSmoothing { get { return swayMovementSmoothing; } set { swayMovementSmoothing = value; } }

    Vector3 newWeaponRotation;
    Vector3 newWeaponVelocity;
    Vector3 targetWeaponRotation;
    Vector3 targetWeaponVelocity;
    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementVelocity;
    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementVelocity;

    Vector3 initialWeaponRotation;
    public Transform weaponHand;
    [Header("Other Variables")]
    public CharacterCollisions character_collisions;
    public InputHandler input_handler;
    [SerializeField]
    private LimbIK leftArm = null;
    [SerializeField]
    private LimbIK rightArm = null;
    public RagdollUtility ragdoll = null;

    [SerializeField]
    private Transform FPCamera = null;
    [SerializeField]
    private GameObject FPCameraObject = null;
    [SerializeField]
    private Transform grappleOrigin = null;
    [SerializeField]
    public GameObject tongue = null;
    [SerializeField]
    private GameObject tongueEnd = null;
    private SplineMesh.Spline tongueSpline;
    private SplineMesh.Spline tongueEndSpline;
    [SerializeField]
    private GameObject leafTarget;
    [SerializeField]
    private Transform leafTargetSpawnLocation;
    [SerializeField]
    private LimbIK leaf = null;
    [SerializeField]
    private ConfigurableJoint leafJoint;
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
    public Text debug_text_action;

    [SerializeField]
    public Text player_health_box;
    [SerializeField]
    public Text debug_speed;
    public bool stopGrapple = false;
    private float setGrappleDistance = 0f;
    private int grappleDirection;
    public Vector3 lastWallNormal = Vector3.zero;
    public Anim curAnimState;
    public Anim curAnimStateThirdPerson;
    public NetworkVariable<Vector3> playerPos;
    public Vector3 oldPlayerPos;
    public Camera UICamera;
    public Camera PlayerCamera;
    public InputActionAsset inputAsset;
    [SerializeField]
    private SkinnedMeshRenderer FPSArms;
    [SerializeField]
    private SkinnedMeshRenderer FPSBlowDart;
    public NetworkVariable<Vector3> cameraRotation;
    public NetworkVariable<Vector3> hipPos;
    public NetworkVariable<bool> crouchActive;
    public NetworkVariable<bool> tongueActive;

    [SerializeField]
    public BlowDart blowDart;
    [SerializeField]
    private NetworkVariable<float> playerHealth;
    private float lastHealth;
    void UpdateMouseLook(){
      
      //get a simple vector 2 for the mouse delta 
      Vector2 mouse_delta = input_handler.mouse_delta;

      //set the y rotation for the camera, user the mouse sensitivity 
      camera_pitch -= mouse_delta.y * mouseSensitivity;

      //clamp the rotation
      camera_pitch = Mathf.Clamp(camera_pitch,-90, 90);

      //set the camera rotation
      UpdatePlayerLookAtServerRpc(new Vector3(camera_pitch, 0, current_camera_roll));
      //FPCamera.localEulerAngles = cameraRotation.Value;
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
        SetAnimationThirdPerson(Anim.WallRunningRight);
      } else {
        leftArm.enabled = true;
        SetAnimationThirdPerson(Anim.WallRunningLeft);
      }
    }

    public void DisableWallRunArms(){
      leftArm.enabled = false;
    }

    public void EnableAimArm(){
      rightArm.enabled = true;
    }

    public void DisbaleAimArm(){
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
        if(!slideTimerSet && velocity.magnitude > maxCrouchSpeed 
          && slide_speed > maxCrouchSpeed){
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
      //print(target_velocity.magnitude + " | " + !slideTimer.is_active);
      if(velocity.magnitude <= maxCrouchSpeed 
      && target_velocity.magnitude <= maxCrouchSpeed
      && !slideTimer.is_active){
        // if(curAnimStateThirdPerson != Anim.Running){
        //   SetAnimationThirdPerson(Anim.Running);
        // }
        if(velocity.magnitude < 2f && curAnimState != Anim.Idle){
            SetAnimationThirdPerson(Anim.Idle);
        } else if(velocity.magnitude >= 2f && curAnimState != Anim.Running) {
            SetAnimationThirdPerson(Anim.Running);
        }
        //take slope into account
        Vector3 slope_direction = Vector3.ProjectOnPlane(input_direction, character_collisions.ground_slope);
        //Get the speed you want to get to 
        Vector3 crouch_velocity = slope_direction * maxCrouchSpeed * speed_mod;
        //Smoothly transition to that velocity 
        velocity = Vector3.Lerp(velocity, crouch_velocity, crouchAccSpeed * Time.fixedDeltaTime);
      }  else {
        if(curAnimStateThirdPerson != Anim.Sliding){
          SetAnimationThirdPerson(Anim.Sliding);
        }
        velocity = Vector3.Lerp(velocity, target_velocity, slideAccSpeed * Time.fixedDeltaTime);
      }
    }

    public void SetSlideValues(){
      //this will give an initial burst of speed 
      last_slide_speed = velocity.magnitude * 1.3f;

      //current forward 
      last_slide_direction = transform.TransformDirection(Vector3.forward);

      CrouchEffector(true);
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
      
      //default target linear velocity 
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

    public void MantleMovement(Vector3 height){
      if(transform.position.y < height.y){
        //calculate which way to climb by using the up vector projected on the wall
        Vector3 target_velocity = (Vector3.ProjectOnPlane(Vector3.up, wall_hit_normal) * maxClimbSpeed / 2) - wall_hit_normal;
        velocity = Vector3.Lerp(velocity, target_velocity, runAccSpeed * Time.fixedDeltaTime);
      }
    }

    public void SetCharacterHeight(bool force, float height){
      if(force){
        controller.height = height;
        controller.center = Vector3.up * height * .5f;
        FPCamera.transform.localPosition = Vector3.up * height * cameraHeightRatio;
        //thirdPerson.localPosition = Vector3.up * .3f * cameraHeightRatio;
      } else {
        controller.height = Mathf.Lerp(controller.height, height, crouchSharpness * Time.fixedDeltaTime);
        controller.center = Vector3.up * height * .5f;
        FPCamera.transform.localPosition = Vector3.Lerp(FPCamera.transform.localPosition, Vector3.up * height * cameraHeightRatio, crouchSharpness * Time.fixedDeltaTime);
        thirdPerson.localPosition = Vector3.Lerp(thirdPerson.localPosition, Vector3.down *(.6f - (.3f * height)), crouchSharpness * Time.fixedDeltaTime);
        UpdateHipServerRpc(hipIK.transform.localPosition);
        if(height > 1.5){
          //grounderBipedIK.solver.liftPelvisWeight = Mathf.Lerp(grounderBipedIK.solver.liftPelvisWeight, .5f, crouchSharpness * Time.fixedDeltaTime);
          hipIK.transform.localPosition = Vector3.Lerp(hipIK.transform.localPosition, standingHips.transform.localPosition, crouchSharpness * Time.fixedDeltaTime);
        } else {
         // grounderBipedIK.solver.liftPelvisWeight = Mathf.Lerp(grounderBipedIK.solver.liftPelvisWeight, -1.5f, crouchSharpness * Time.fixedDeltaTime);
          hipIK.transform.localPosition = Vector3.Lerp(hipIK.transform.localPosition, crouchingHips.transform.localPosition, crouchSharpness * Time.fixedDeltaTime);
        }
        
        //sync a network variable for this hip ik here

        //grounderBipedIK.solver.Update();
        //grounderBipedIK.solver.liftPelvisWeight = Mathf.Lerp(grounderBipedIK.solver.liftPelvisWeight, height - 1.5f, crouchSharpness * Time.fixedDeltaTime);
      }
    }

    public void CrouchEffector(bool toggle){
      UpdateCrouchingServerRpc(toggle);
      hipIK.GetComponent<OffsetModifier>().enabled = toggle;
    }

    
    public void SetThirdPersonHeight(bool force, float height){}

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

    public void SetDebugTextAction(string t){
      if(debug_text_action != null){
        debug_text_action.text = t;
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
      print((action_machine.cur_state == idle_state) + " " +
      movement_machine.cur_state != grappling_state + " " +
      !GrappleCooldownTimer.is_active);
      if(action_machine.cur_state == idle_state &&
      movement_machine.cur_state != grappling_state && 
      !GrappleCooldownTimer.is_active){
       stopGrapple = false;
        movement_machine.ChangeState(grappling_state);
        UpdateTongueServerRpc(true);
      }
    }
    void OnGrappleUp(){
      if(movement_machine.cur_state == grappling_state){
        stopGrapple = true;
        GrappleCooldownTimer.StartTimer();
      }
    }

    void OnShootDown(){
      //blowDart.HandleShootInputs(true, false, false);
      if(IsClient && action_machine.cur_state != reload_state){
      ShootDartServerRpc(true, false, false);
      }
    }
    void OnShootUp(){
      //blowDart.HandleShootInputs(false, false, true);
      if(IsClient){
      ShootDartServerRpc(false, false, true);
      }
    }

    public void SetAnimation(Anim index){
      animator.SetInteger("Change", ((int)index));
      curAnimState = index;
    }

    public void SetAnimationThirdPerson(Anim index){
      curAnimStateThirdPerson = index;
      UpdatePlayerStateServerRpc(index);
      animatorThirdPerson.SetInteger("Change", ((int)netowrkAnimationState.Value));
      
    }

    public void SetSenitivity(float val){
      mouseSensitivity = val;
    }

    public void WeaponSway(){
      targetWeaponRotation.y += swayAmount * input_handler.mouse_delta.x * Time.fixedDeltaTime;
      targetWeaponRotation.x += swayAmount * input_handler.mouse_delta.y * Time.fixedDeltaTime;

      targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, initialWeaponRotation.x - SwayClampX, initialWeaponRotation.x + swayClampX);
      targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, initialWeaponRotation.y - SwayClampY, initialWeaponRotation.y + swayClampY);
      targetWeaponRotation.z = initialWeaponRotation.z +  ((targetWeaponRotation.y -  initialWeaponRotation.y)* 5);

      targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, initialWeaponRotation, ref targetWeaponVelocity, swayResetSmoothing);
      newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponVelocity, swaySmoothing);
      
      targetWeaponMovementRotation.z = input_handler.move_input.x * swayMovementX;
      targetWeaponMovementRotation.x = input_handler.move_input.z * swayMovementY;

      targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementVelocity, swayMovementSmoothing);
      newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementVelocity, swayMovementSmoothing);
      
      weaponHand.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }
    
    public void ReticleBloom(bool aiming){
      //Check if player is currently moving and Lerp currentSize to the appropriate value.
      if(aiming) {
        currentSize = Mathf.Lerp(currentSize, restingSize * .6f, Time.fixedDeltaTime * aimSpeed);
      } else if(velocity.magnitude < 5) {
        currentSize = Mathf.Lerp(currentSize, restingSize, Time.fixedDeltaTime *  5f);
      } else {
        currentSize = Mathf.Lerp(currentSize, maxSize, Time.fixedDeltaTime * velocity.magnitude);
      }

      reticle.sizeDelta = new Vector2(currentSize, currentSize);
    }

    public void UpdateServer(){
      // go to the new position on the server side 
      //transform.position = playerPos.Value; //Vector3.ProjectOnPlane(playerPos.Value, Vector3.up);
      //controller.Move(playerPos.Value * Time.fixedDeltaTime);
    }

    public void UpdateClient(){
      if(oldPlayerPos != playerPos.Value){
        oldPlayerPos = playerPos.Value;
        //UpdateClientPositionServerRpc(playerPos.Value);
      }
    }

    public void InflictDamage(float damage){
      MonoBehaviour.print("inflict dame pre rpc");
      UpdateClientHealthServerRpc(playerHealth.Value - damage);
      //if(!IsLocalPlayer){
        player_health_box.text = playerHealth.Value.ToString();
      //}
    }


    [ServerRpc (RequireOwnership = false)]
    private void UpdateClientHealthServerRpc(float health)
    {
        playerHealth.Value = health;
    }
    [ServerRpc]
    private void UpdateClientPositionServerRpc()
    {
        playerPos.Value = velocity;
        controller.Move(playerPos.Value * Time.fixedDeltaTime);
    }

    [ServerRpc (RequireOwnership = false)]
    private void UpdatePlayerStateServerRpc(Anim animState)
    {
        netowrkAnimationState.Value = animState;
    }

    [ServerRpc]
    private void UpdatePlayerLookAtServerRpc(Vector3 pos)
    {
        cameraRotation.Value = pos;
    }

    [ServerRpc]
    private void UpdateHipServerRpc(Vector3 pos)
    {
        hipPos.Value = pos;
    }

    [ServerRpc]
    private void UpdateCrouchingServerRpc(bool toggle)
    {
        crouchActive.Value = toggle;
    }
    [ServerRpc]
    private void ShootDartServerRpc(bool inputDown, bool inputHeld, bool inputUp)
    {
        blowDart.HandleShootInputs(inputDown, inputHeld, inputUp);
    }

    [ServerRpc]
    public void UpdateTongueServerRpc(bool toggle)
    {
        tongueActive.Value = toggle;
    }

    private void Awake() {
      Application.targetFrameRate = 120;
    }
    public override void OnNetworkSpawn()
    {
      base.OnNetworkSpawn();
      GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);

      Vector3 pos = leafTargetSpawnLocation.transform.position;
      GameObject leaf_temp = Instantiate(leafTarget,pos, Quaternion.identity);
      leaf.solver.target = leaf_temp.transform;
      leaf_temp.transform.position = leafTargetSpawnLocation.position;
      leafJoint.connectedBody = leaf_temp.GetComponent<Rigidbody>();

      controller = GetComponent<CharacterController>();
      character_collisions = GetComponent<CharacterCollisions>();
      input_handler = GetComponent<InputHandler>();
      tongueSpline = tongue.GetComponent<SplineMesh.Spline>();
      tongueEndSpline = tongueEnd.GetComponent<SplineMesh.Spline>();

      SetAnimation(((int)Anim.Idle));
      standing_height = controller.height;
      crouching_height = standing_height / 2;

      initialWeaponRotation = weaponHand.localRotation.eulerAngles;
      targetWeaponRotation = initialWeaponRotation;

      // initialize all state machine variables
      movement_machine = new StateMachine();
      running_state = new RunningState(this, movement_machine);
      falling_state = new FallingState(this, movement_machine);
      sliding_state = new SlidingState(this, movement_machine);
      wall_climbing_state = new WallClimbingState(this, movement_machine);
      wall_running_state = new WallRunningState(this, movement_machine);
      grappling_state = new GrapplingState(this, movement_machine);
      mantle_state = new MantleState(this, movement_machine);
      ragdoll_state = new RagdollState(this, movement_machine);

      action_machine = new StateMachine();
      locked_state = new LockedState(this, action_machine);
      grapple_state = new GrappleState(this, action_machine);
      idle_state = new IdleState(this, action_machine);
      reload_state = new ReloadState(this, action_machine);
      shoot_state = new ShootState(this, action_machine);

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
      
      //default to the falling state 
      movement_machine.Initialize(falling_state);
      action_machine.Initialize(idle_state);

      //lock cursor to screen and hide cursor
      if(lock_cursor){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
      }
      var camera = FPCameraObject.GetComponent<Camera>();

      lastHealth = playerHealth.Value;
      if((IsOwner)|| SceneManager.GetActiveScene().name.Equals("SampleScene")){
        gameObject.GetComponent<PlayerInput>().actions = inputAsset;
        player_health_box.text = 5f.ToString();
        UpdateClientHealthServerRpc(5);
        //gameObject.GetComponent<PlayerInput>().
        //gameObject.GetComponent<InputHandler>().enabled = true;
        // input_handler = gameObject.AddComponent<InputHandler>();
        // input_handler.UICamera = UICamera;
        // input_handler.PlayerCamera = PlayerCamera;
        //gameObject.GetComponent<PlayerInputManager>().enabled = true;
  
      } else {
        //FPCameraObject.SetActive(false);
        gameObject.GetComponent<PlayerInput>().DeactivateInput();
        //gameObject.GetComponent<InputHandler>().enabled = false;
        SetLayerRecursively(gameObject, "EnemyPlayer");
        //objectThirdPerson.layer = LayerMask.NameToLayer("FPS");
        //SetLayerRecursively(objectThirdPerson, "FPS");
        FPCameraObject.GetComponent<Camera>().enabled = false;
        FPCameraObject.GetComponent<AudioListener>().enabled = false;
        FPSArms.enabled = false;
        FPSBlowDart.enabled = false;
        //gameObject.GetComponent<PlayerInputManager>().enabled = false;
      }

      // if(IsLocalPlayer){
      //   gameObject.AddComponent<ClientNetworkTransform>();
      // } else {
      //   gameObject.AddComponent<NetworkTransform>();
      // }
    }

    void SetLayerRecursively(GameObject obj, string newLayer)
    {
        if (null == obj)
        {
            return;
        }
       
        obj.layer = LayerMask.NameToLayer(newLayer);
       
        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    void Update()
    {
      if(IsLocalPlayer || SceneManager.GetActiveScene().name.Equals("SampleScene")){
      //always update aiming
      UpdateMouseLook();
      if(!jumpCooldownTimer.is_active && !wallJumpCooldownTimer.is_active){
        can_jump = JumpBuffer();
      }

      // //temporary handlers for grapple and ragdoll state, need to be stripped out later 
      // if(input_handler.is_grappling){
      //   if(movement_machine.cur_state != grappling_state){
      //     movement_machine.ChangeState(grappling_state);
      //   }
      // } 
      
      if(input_handler.is_ragdoll){
        movement_machine.ChangeState(ragdoll_state);
      }

      movement_machine.cur_state.HandleInput();
      movement_machine.cur_state.LogicUpdate();

      action_machine.cur_state.HandleInput();
      action_machine.cur_state.LogicUpdate();

      if(lastHealth != playerHealth.Value){
        lastHealth = playerHealth.Value;
        player_health_box.text = playerHealth.Value.ToString();
      }
      } else {
        animatorThirdPerson.SetInteger("Change", ((int)netowrkAnimationState.Value));
        FPCamera.localEulerAngles = cameraRotation.Value;
        if(crouchActive.Value ){
          //hipIK.transform.localPosition = standingHips.localPosition;
          hipIK.GetComponent<OffsetModifier>().weight = 1;
        } else if(!crouchActive.Value){
          hipIK.GetComponent<OffsetModifier>().weight = 0;
        }
        
        hipIK.transform.localPosition = hipPos.Value;
        if(tongueActive.Value){
          if(!tongue.activeSelf){
            //tongue.SetActive(true);
            StartGrapple();
          }
          SetTongue();
        } else if(tongue.activeSelf ){
          tongue.SetActive(false);
        }
        // if( standingHips.localPosition.y - hipPos.Value.y > 2){
        //   hipIK.GetComponent<OffsetModifier>().enabled = true;
        // } else {
        //   hipIK.GetComponent<OffsetModifier>().enabled = false;
        // }
        // if(crouchActive.Value){
        //   if(hipPos.Value.y - standingHips.localPosition.y < 5){
        //     hipIK.GetComponent<OffsetModifier>().enabled = crouchActive.Value;
        //   }
        // } else {
        //   hipIK.GetComponent<OffsetModifier>().enabled = crouchActive.Value;
        // }
      }
    }

    void FixedUpdate() {
    //local player should be moving according to player logic
    if(IsLocalPlayer|| SceneManager.GetActiveScene().name.Equals("SampleScene")){
      // tongueSpline.nodes[0].Position = grappleOrigin.localPosition;
      // tongueSpline.nodes[0].Direction = grappleOrigin.localPosition + (FPCamera.TransformDirection(Vector3.forward) *5);
    
      // Vector3 tongue_end = tongue.transform.InverseTransformPoint(testPosition.position);
      // tongueSpline.nodes[1].Position =  tongue_end;
      // Vector3 scale = new Vector3(.2f, .2f, .2f);
      // tongueSpline.nodes[1].Direction = tongue_end +  (Vector3.Cross(testPosition.TransformDirection(Vector3.forward), Vector3.up)* 5);//Vector3.Scale(grappleHit.normal * -1, scale);
      //input_direction = input_handler.move_input;

      //translate inputs to the correct values for calculations later
      input_direction = transform.right * input_handler.move_input.x + transform.forward * input_handler.move_input.z;

      //set animations all the time for now
      animatorThirdPerson.SetFloat("x", input_handler.move_input.x, .5f, Time.fixedDeltaTime);
      animatorThirdPerson.SetFloat("y", input_handler.move_input.z, .5f, Time.fixedDeltaTime);
      WeaponSway();
      //animatorThirdPerson.SetFloat("Velocity", controller.velocity.magnitude / 5);
      
      //calls current machine states
      movement_machine.cur_state.PhysicsUpdate();
      action_machine.cur_state.PhysicsUpdate();

      //reticle always updating
      //ReticleBloom(false);

        //uses the movement calculated by the current state 
        controller.Move(velocity * Time.fixedDeltaTime);
        //UpdateClientPositionServerRpc();
        //UpdateClient();
        if(debug_speed != null){
          debug_speed.text =  ((int)(((new Vector3(velocity.x, 0 , velocity.z).magnitude / 1000) * 60) * 60)).ToString();
        }
        //print(Vector3.Dot(input_direction, character_collisions.last_wall_normal) + " | " +input_direction + " | " +  character_collisions.last_wall_normal);
      }

      if(IsServer){
        UpdateServer();
      }
    }
}

public enum Anim : int{Idle, Running, Sliding, Falling, Climbing, WallRunningLeft, WallRunningRight}
