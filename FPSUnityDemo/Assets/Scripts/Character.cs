using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class Character : MonoBehaviour
{
    public State running_state;
    public State falling_state;
    public State wall_running_state;
    public State sliding_state;
    public State crouching_state;
    public StateMachine movement_machine;
    public CharacterCollisions character_collisions;
    public InputHandler input_handler;

    [SerializeField]
    private Transform FPCamera = null;
    [SerializeField]
    private float mouse_sensitivity = 1.0f;
 
    [SerializeField]
    private float gravity = 1.0f;
    [SerializeField]
    private float air_acc = 1.0f;
    [SerializeField]
    private float max_air_speed = 1.0f;
    [SerializeField]
    private float jump_force = 1.0f;
    [SerializeField]
    private float max_speed = 1.0f;
    [SerializeField]
    private float acc_speed = 1.0f;
    [SerializeField]
    private float max_crouch_speed = 1.0f;
    [SerializeField]
    private float crouch_acc_speed = 1.0f;
    [SerializeField]
    private float wall_speed_mod = 1.0f;
    [SerializeField]
    private float wall_run_gravity = 1.0f;
    [SerializeField]
    private float max_slide_speed = 1.0f;
    [SerializeField]
    private float max_climb_speed = 1.0f;
    [SerializeField]
    private float base_slide_speed = 1.0f;
    [SerializeField]
    private float slide_acc_speed = 1.0f;
    
    public float max_angle_roll = 1.0f;
    [SerializeField]
    private float angle_roll_speed = 1.0f;
    [SerializeField]
    private float crouching_sharpness = 10f;
    [SerializeField]
    public CharacterController controller = null;
    public bool can_wall_run = true;
    [SerializeField]
    private float camera_height_ratio = .9f;
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
    public Timer slide_timer;
    [SerializeField]
    private float slide_time;
    public Timer jump_cooldown_timer;
    [SerializeField]
    private float jump_cooldown_time;
    public Timer jump_buffer_timer;
    [SerializeField]
    private float jump_buffer_time;
    public Timer coyote_timer; 
    [SerializeField]
    private float coyote_time;
    public float standing_height;
    public float crouching_height;
    private bool slide_timer_set = false;
    public bool can_jump;
    public float current_camera_roll;
    
    void UpdateMouseLook(){
      //get a simple vector 2 for the mouse delta 
      //Vector2 mouse_delta = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
      Vector2 mouse_delta = input_handler.mouse_delta;

      //set the y rotation for the camera, user the mouse sensitivity 
      camera_pitch -= mouse_delta.y * mouse_sensitivity;

      //clamp the rotation
      camera_pitch = Mathf.Clamp(camera_pitch,-90, 90);

      //set the camera rotation
      FPCamera.localEulerAngles = new Vector3(camera_pitch, 0, current_camera_roll); 

      //apply this rotation with the sensitivity modifier 
      transform.Rotate(Vector3.up * mouse_delta.x * mouse_sensitivity);
    }

    public void GroundMovement(){
      //sprint modifier applied 
      speed_mod = input_handler.is_sprinting ? 1.3f : 1f;
      
      //take slop into account
      Vector3 slope_direction = Vector3.ProjectOnPlane(input_direction, character_collisions.ground_slope);
      //Get the speed you want to get to 
      Vector3 target_velocity = slope_direction * max_speed * speed_mod;
      //Smoothly transition to that velocity 
      velocity = Vector3.Lerp(velocity, target_velocity, acc_speed * Time.fixedDeltaTime);
    }

    public void AirMovement(){
      speed_mod = input_handler.is_sprinting ? 1.3f : 1f;
      
      //Add the air acceleration to the velocity
      velocity += input_direction * air_acc * Time.fixedDeltaTime;

      //clamp sideways velocity, keep upward velocity
      float vertical_velocity = velocity.y;
      Vector3 horizontal_velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
      horizontal_velocity = Vector3.ClampMagnitude(horizontal_velocity, max_air_speed * speed_mod);
      velocity = horizontal_velocity + (Vector3.up * vertical_velocity);

      // now add gravity acceleration
      velocity += Vector3.down * gravity * Time.fixedDeltaTime;
    }

    public void WallRun(){
      if(velocity.y > 0){
        //Keep upward velocity if it positive
        float vertical_velocity = velocity.y;
        Vector3 horizontal_velocity = Vector3.ProjectOnPlane(orthogonal_wall_vector * wall_direction * wall_speed_mod, Vector3.up);
        velocity = horizontal_velocity + (Vector3.up * vertical_velocity);
      } else{
        //make the velocity equal the correct direction and add the wall speed modifier 
        velocity = orthogonal_wall_vector * wall_direction * wall_speed_mod;
      }     

      // now apply gravity so there is a downward arc 
      velocity += Vector3.down * wall_run_gravity * Time.fixedDeltaTime;
    }

    public void WallClimb(){
       Vector3 target_velocity = Vector3.up * max_climb_speed;

      velocity = Vector3.Lerp(velocity, target_velocity, acc_speed * Time.fixedDeltaTime);
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
        if(!slide_timer_set){
          slide_timer.Start();
          slide_timer_set = true; 
        }
        if(!slide_timer.is_active){
          slide_speed = 1f;
        }
      } else {
        if(slide_timer_set){
          slide_timer_set = false; 
        }
        // parallel to ground
        Vector3 ground_orthogonal = Vector3.Cross(transform.up, character_collisions.ground_slope);
        //parallel to ground with the proper slope direction 
        Vector3 slope_orthogonal = Vector3.Cross(ground_orthogonal, character_collisions.ground_slope);
        //parallel to ground with current transform forward 
        slide_direction = Vector3.ProjectOnPlane(velocity.normalized + slope_orthogonal.normalized, character_collisions.ground_slope).normalized; 

        slide_speed = (1 + (current_slope / controller.slopeLimit)) * base_slide_speed;
        slide_speed = Mathf.Min(slide_speed, max_slide_speed);
        //slide_speed =  (1f - character_collisions.ground_slope.y) * );
        last_slide_direction = slide_direction;
        last_slide_speed = slide_speed; 
      }
      Debug.DrawRay (transform.position, slide_direction * 10, Color.green);
      
      target_velocity = slide_direction * slide_speed;

      
      // perform a crouch if your current velocity is less than half your run speed along with your target velocity 
      if(velocity.magnitude <= max_crouch_speed 
      && target_velocity.magnitude <= max_crouch_speed
      && !slide_timer.is_active){
        //take slope into account
        Vector3 slope_direction = Vector3.ProjectOnPlane(input_direction, character_collisions.ground_slope);
        //Get the speed you want to get to 
        Vector3 crouch_velocity = slope_direction * max_crouch_speed * speed_mod;
        //Smoothly transition to that velocity 
        velocity = Vector3.Lerp(velocity, crouch_velocity, crouch_acc_speed * Time.fixedDeltaTime);
      }  else {
        velocity = Vector3.Lerp(velocity, target_velocity, slide_acc_speed * Time.fixedDeltaTime);
      }
    }

    public void SetSlideValues(){
      last_slide_speed = velocity.magnitude * 1.3f;
      last_slide_direction = transform.TransformDirection(Vector3.forward);
    }

    public bool CrouchThreshold(){
      return velocity.magnitude <= max_crouch_speed;
    }
    public void ResetSlideTimer(){
      slide_timer_set = false;
      slide_timer.Stop();
    }

    public void SetWallRunValues(){      
      //get current current forward direction for character
      Vector3 along_wall = transform.TransformDirection(Vector3.forward);

      //get the wall normal, needs to be set for jumping
      wall_hit_normal = character_collisions.WallHitNormal();

      //get the orthogonal vector from the wall compared to UP 
      orthogonal_wall_vector = Vector3.Cross(wall_hit_normal, Vector3.up);
      
      //use the dot product to determine which direction the player is facing on the wall
      wall_direction = Vector3.Dot(along_wall, orthogonal_wall_vector) < 0 ? -1 : 1; 
    }

    public void WallJump(){
      jump_cooldown_timer.Start();
      ResetJumpBuffer();
      // start by canceling out the vertical component of our velocity
      velocity = new Vector3(velocity.x, 0f, velocity.z);
      
      // then, add the jumpSpeed value in the wall direction
      velocity += (wall_hit_normal + Vector3.up) * jump_force;
    }
    public void GroundJump(){
      jump_cooldown_timer.Start();
      ResetJumpBuffer();
      // start by canceling out the vertical component of our velocity
      //velocity = new Vector3(velocity.x, 0f, velocity.z);
      velocity = Vector3.ProjectOnPlane(new Vector3(velocity.x, 0f, velocity.z), character_collisions.ground_slope);
      // then, add the jumpSpeed value upwards
      velocity += character_collisions.ground_slope * jump_force;
    }

    public void GroundWallJump(){
      jump_cooldown_timer.Start();
      ResetJumpBuffer();
      // start by canceling out the vertical component of our velocity
      //velocity = new Vector3(velocity.x, 0f, velocity.z);
      velocity = Vector3.ProjectOnPlane(new Vector3(velocity.x, 0f, velocity.z), Vector3.Cross(character_collisions.WallHitNormal(), Vector3.up));
      // then, add the jumpSpeed value upwards
      velocity += character_collisions.ground_slope * jump_force;
    }

    public void SetCharacterHeight(bool force, float height){
      if(force){
        controller.height = height;
        controller.center = Vector3.up * height * .5f;
        FPCamera.transform.localPosition = Vector3.up * height * camera_height_ratio;
      } else {
        controller.height = Mathf.Lerp(controller.height, height, crouching_sharpness * Time.fixedDeltaTime);
        controller.center = Vector3.up * height * .5f;
        FPCamera.transform.localPosition = Vector3.Lerp(FPCamera.transform.localPosition, Vector3.up * height * camera_height_ratio, crouching_sharpness * Time.fixedDeltaTime);
      }
    }

    public void SetCameraAngle(float target_angle){
      float camera_angle = FPCamera.eulerAngles.z;

      current_camera_roll = Mathf.LerpAngle(camera_angle,target_angle, angle_roll_speed * Time.fixedDeltaTime);
    }

    public bool AtHeight(float height){
      return controller.height == height;
    }
    

    private bool JumpBuffer(){
      if(Keyboard.current.spaceKey.wasPressedThisFrame){
        if(jump_buffer_timer.is_active){
          jump_buffer_timer.Stop();
          jump_buffer_timer.Start();
        } else {
          jump_buffer_timer.Start();
        }
      }

      return (jump_buffer_timer.is_active );
    }
    private void ResetJumpBuffer(){
      jump_buffer_timer.Stop();
      can_jump = false;
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
      crouching_state = new CrouchingState(this, movement_machine);
      wall_running_state = new WallRunningState(this, movement_machine);

      //initialize timers to set values
      slide_timer = gameObject.AddComponent<Timer>();
      slide_timer.SetTimer(slide_time);
      jump_buffer_timer = gameObject.AddComponent<Timer>();
      jump_buffer_timer.SetTimer(jump_buffer_time);
      jump_cooldown_timer = gameObject.AddComponent<Timer>();
      jump_cooldown_timer.SetTimer(jump_cooldown_time);
      coyote_timer = gameObject.AddComponent<Timer>();
      coyote_timer.SetTimer(coyote_time);
      
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
      movement_machine.cur_state.HandleInput();
      movement_machine.cur_state.LogicUpdate();
    }

    void FixedUpdate() {
      
      //input_direction = input_handler.move_input;
      input_direction = transform.right * input_handler.move_input.x + transform.forward * input_handler.move_input.z;
      movement_machine.cur_state.PhysicsUpdate();
      controller.Move(velocity * Time.fixedDeltaTime);

    }
}
