using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public State running_state;
    public State falling_state;
    public State wall_running_state;
    public StateMachine movement_machine;

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
    public CharacterController controller = null;
    [SerializeField]
    private bool lock_cursor;
    
    private float camera_pitch = 0;

    private Vector3 velocity;
    private Vector3 input_direction;
    private Vector3 test;
    void Start()
    {
      controller = GetComponent<CharacterController>();
      
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

    void UpdateMouseLook(){
      //get a simple vector 2 for the mouse delta 
      Vector2 mouse_delta = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));

      //set the y rotation for the camera, user the mouse sensitivity 
      camera_pitch -= mouse_delta.y * mouse_sensitivity;

      //clamp the rotation
      camera_pitch = Mathf.Clamp(camera_pitch,-90, 90);

      //set the camera rotation
      FPCamera.localEulerAngles = Vector3.right * camera_pitch;

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

    public void GroundJump(){
      // start by canceling out the vertical component of our velocity
      velocity = new Vector3(velocity.x, 0f, velocity.z);
      
      // then, add the jumpSpeed value upwards
      velocity += Vector3.up * jump_force;
    }
}
