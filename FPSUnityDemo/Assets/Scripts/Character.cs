using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public State running_state;
    public State falling_state;
    public State wall_running_state;
    public StateMachine movement_machine;
    public CharacterCollisions character_collisions;

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
    private float speed_mod = 1.0f;
    [SerializeField]
    private float acc_speed = 1.0f;
    [SerializeField]
    private float wall_speed_mod = 1.0f;
    [SerializeField]
    private float wall_run_gravity = 1.0f;
    [SerializeField]
    public CharacterController controller = null;
    [SerializeField]
    public bool can_wall_run = true;
    public bool lock_cursor;
    
    private float camera_pitch = 0;

    public Vector3 velocity;
    private Vector3 input_direction;
    private Vector3 test;
    private Vector3 hit_normal;
    private Vector3 orthogonal_wall_vector;
    private int wall_direction;
    
    void UpdateMouseLook(){
      //get a simple vector 2 for the mouse delta 
      Vector2 mouse_delta = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));

      //set the y rotation for the camera, user the mouse sensitivity 
      camera_pitch -= mouse_delta.y * mouse_sensitivity;

      //clamp the rotation
      camera_pitch = Mathf.Clamp(camera_pitch,-90, 90);

      //set the camera rotation
      FPCamera.localEulerAngles = new Vector3(camera_pitch, 0, 0); 

      //apply this rotation with the sensitivity modifier 
      transform.Rotate(Vector3.up * mouse_delta.x * mouse_sensitivity);
    }

    public void GroundMovement(){
      //Get the speed you want to get to 
      Vector3 target_velocity = input_direction * max_speed * speed_mod;

      //Smoothly transition to that velocity 
      velocity = Vector3.Lerp(velocity, target_velocity, acc_speed * Time.fixedDeltaTime);
    }

    public void AirMovement(){
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

    public void SetWallRunValues(){      
      //get current current forward direction for character
      Vector3 along_wall = transform.TransformDirection(Vector3.forward);

      //get the wall normal, needs to be set for jumping
      hit_normal = character_collisions.HitNormal();

      //get the orthogonal vector from the wall compared to UP 
      orthogonal_wall_vector = Vector3.Cross(hit_normal, Vector3.up);
      
      //use the dot product to determine which direct the player is facing on the wall
      wall_direction = Vector3.Dot(along_wall, orthogonal_wall_vector) < 0 ? -1 : 1; 
    }

    public void WallJump(){
      // start by canceling out the vertical component of our velocity
      velocity = new Vector3(velocity.x, 0f, velocity.z);
      
      // then, add the jumpSpeed value in the wall direction
      velocity += (hit_normal + Vector3.up) * jump_force;
    }
    public void GroundJump(){
      // start by canceling out the vertical component of our velocity
      velocity = new Vector3(velocity.x, 0f, velocity.z);
      
      // then, add the jumpSpeed value upwards
      velocity += Vector3.up * jump_force;
    }

    void Start()
    {
      controller = GetComponent<CharacterController>();
      character_collisions = GetComponent<CharacterCollisions>();
      // initialize all state machine variables
      movement_machine = new StateMachine();
      running_state = new RunningState(this, movement_machine);
      falling_state = new FallingState(this, movement_machine);
      wall_running_state = new WallRunningState(this, movement_machine);

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
      movement_machine.cur_state.HandleInput();
      movement_machine.cur_state.LogicUpdate();
    }

    void FixedUpdate() {
      float x = Input.GetAxisRaw("Horizontal");
      float z = Input.GetAxisRaw("Vertical");
      
      input_direction = transform.right * x + transform.forward * z;
      movement_machine.cur_state.PhysicsUpdate();
      controller.Move(velocity * Time.fixedDeltaTime);
    }
}
