// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;


// public class Character 
// {
//     public StateMachine movement_machine;
//     public State standing_state;
//     public State jumping_state;
//     public State falling_state;
//     public State swinging_state;
//     public State ground_sliding_state;

//     public StateMachine action_machine;    
//     public State aiming_state;
//     public State grappling_state;
//     public State idle_state;
//     public LineRenderer jump_arc;
//     public Animator animations;
//     [SerializeField]
//     private float jump_speed = 40f;
//     [SerializeField]
//     private float speed = 1f;
//     [SerializeField]
//     private float maxRunSpeed = 5f;
//     [SerializeField]
//     private float runAccSpeed = 11f;
//     [SerializeField]
//     private float dec_speed = 11f;
//     [SerializeField]


//     private float gravity = -20f;

//     public PlayerCollision collision;
//    [SerializeField]
//     private LayerMask is_grappleable;

//     public Rigidbody rigid_body;
//     private ConfigurableJoint joint;
//     private ConfigurableJoint player_joint;

//     public float initial_tongue_distance { get; private set; }
//     public float cur_tongue_distance { get; private set; }
//     [SerializeField]
//     private Transform mouth;
//     [SerializeField]
//     private Transform main_camera;
//     [SerializeField]
//     private Transform player;
//     private GameObject hit_location;
//     private GameObject player_pivot_location;
//     [SerializeField]
//     private GameObject aim_marker_prefab;
//     private GameObject aim_marker;
//     private MeshRenderer aim_marker_mesh;
//     private RaycastHit camera_hit;
//     private RaycastHit tongue_hit;

//     [SerializeField]
//     private Transform head;
//     [SerializeField]
//     private ConfigurableJoint head_joint;
//     [SerializeField]
//     private ConfigurableJoint spine_joint;
//     [SerializeField]
//     public Transform stabilizer_transform;
//     [SerializeField]
//     private Transform focal_point;
//     private Quaternion head_initial_pos;
//     private Quaternion spine_initial_pos;
//     //private CableComponent cable_component;
//     [SerializeField]
//     private Material tongue_material;
//     private float turnSpeed = .1f;

//     public Transform look_at;

//     private float max_tongue_strength = 1f;

//     private float tongue_dampen = 40f;

//     private float max_tongue_distance;

//     public ParticleSystem explosionEffect;

//     //public Impact spine;

//     public float mass = 30;

//     public float aerial_influence = 500;

//     public enum Anim {Idle, Jump, Air};

//     private Anim current_animation;

//     public Timer standing_cooldown;

//     public void Move(float speed_modifier, Vector3 direction){
//         if(Mathf.Abs(rigid_body.velocity.magnitude) > maxRunSpeed) {
//             rigid_body.velocity = Vector3.MoveTowards(rigid_body.velocity, direction * speed * speed_modifier, dec_speed * Time.deltaTime);
//         } else {
//             rigid_body.velocity = Vector3.MoveTowards(rigid_body.velocity, direction * speed * speed_modifier, runAccSpeed * Time.deltaTime);
//         }
//     }

//     public void Vector(float speed_modifier, Vector3 direction){
//         var vector = direction * speed * speed_modifier;
//         rigid_body.AddForce(Vector3.up * gravity + vector,ForceMode.Acceleration);
//     }

//     public void Fall(){
//         float horizontal_input = Input.GetAxis("Horizontal");
//         float vertical_input = Input.GetAxis("Vertical");
//         Vector3 input_vector = new Vector3(horizontal_input,0,vertical_input);
//         float speed_modifier = input_vector.magnitude * aerial_influence;
//         Vector3 dir = input_vector.normalized;
//         if(input_vector.magnitude >= 0.1f){
//             float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + Camera.main.gameObject.transform.eulerAngles.y;
//             //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSpeed, .1f);
//             Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
//             Vector(speed_modifier, moveDir.normalized);

//             var rotation = new Quaternion(0f,Quaternion.Euler(0f, targetAngle, 0f).y, 0f, 1);

//             ConfigurableJointExtensions.SetTargetRotationLocal(spine_joint, rotation, spine_initial_pos);
//         } else {
//             Vector(1f, Vector3.zero);
//         }
//     }
//     public void Stop(float y_pos){
//       if(collision.sphere_collided){
//         rigid_body.velocity = new Vector3(0, 0, 0);
//         Quaternion target_angle = Quaternion.FromToRotation(player.position,(player.position + collision.closest_hit.direction)); 

//         stabilizer_transform.LookAt(collision.closest_hit.direction, Vector3.up);
//         stabilizer_transform.Rotate( - 90f,0f,0f);
   
//        if(collision.closest_hit.direction.y > -.5f){
//           stabilizer_transform.rotation = new Quaternion(stabilizer_transform.rotation.x, y_pos, stabilizer_transform.rotation.z,stabilizer_transform.rotation.w );
//        }
//       }
//     }
    
//     public void Jump(Vector3 angle){
//         ApplyImpulse(angle * jump_speed);
//     }

//     public void ApplyImpulse(Vector3 force){
//         rigid_body.AddForce(force, ForceMode.Impulse);
//     }
//     public bool StartGrapple(){
//         if(Physics.Raycast(look_at.position, main_camera.forward, out camera_hit, max_tongue_distance, is_grappleable) &&
//           Physics.Raycast(mouth.position, (camera_hit.point- mouth.position).normalized, out tongue_hit, max_tongue_distance, is_grappleable)) {
//             hit_location = new GameObject();
//             hit_location.transform.position = tongue_hit.point;
//             hit_location.transform.parent = tongue_hit.transform;
//             initial_tongue_distance = Vector3.Distance(player.position, hit_location.transform.position);
//             cur_tongue_distance = initial_tongue_distance;

//             // hit_location.AddComponent<CableComponent>();
//             // cable_component = hit_location.GetComponent<CableComponent>();
//             // cable_component.endPoint = mouth;
//             // cable_component.cableMaterial = tongue_material;
//             // cable_component.cableLength = initial_tongue_distance;
//             return true;
//         } else {
//             return false;
//         }
//     }

//     // public void DisableTongue(){
//     //     Destroy(joint);
//     //     Destroy(player_joint);
//     //     Destroy(player_pivot_location);
//     // }

//     // public void StopGrapple(){
//     //     var dir = (focal_point.position - head.position).normalized;
//     //     //head_joint.targetRotation = Quaternion.LookRotation(dir);
//     //     ConfigurableJointExtensions.SetTargetRotationLocal(head_joint,head_initial_pos, head_initial_pos);

//     //     Destroy(joint);
//     //     Destroy(player_joint);
//     //     Destroy(player_pivot_location);
//     //     Destroy(hit_location);
//     // }

//     public void TransitionAnimations(Anim animation_switch){
//       switch(animation_switch){
//         case Anim.Idle:
//           animations.SetInteger("animation_value", 0);
//           break;
//         case Anim.Jump:
//           animations.SetInteger("animation_value", 1);
//           break;
//         case Anim.Air:
//           animations.SetInteger("animation_value", 2);
//           break;
//       }

//       current_animation = animation_switch;
//     }

//     public void UpdateTonguePositions(){
//         cur_tongue_distance = Vector3.Distance(mouth.position, hit_location.transform.position);

//         var dir = (hit_location.transform.position - head.position).normalized;
//         ConfigurableJointExtensions.SetTargetRotationLocal(head_joint, Quaternion.LookRotation(dir), head_initial_pos);
//         // if(cable_component.line != null){
//         //     cable_component.line.SetPosition(cable_component.segments, mouth.position);
//         //     cable_component.cableLength = cur_tongue_distance;
//         // }
//     }

//     public bool ApplyTongueForce() {
//       if (!hit_location) {
//         return false;
//       }
//       Rigidbody hitBody = tongue_hit.collider.GetComponent<Rigidbody>();
//       Character character = tongue_hit.collider.GetComponentInParent<Character>();
//       Vector3 dist = hit_location.transform.position - head.position;
//       Vector3 force = (dist.magnitude > max_tongue_strength) ? dist * max_tongue_strength/dist.magnitude : dist;
//       force *= tongue_dampen;
//       if (character != null) {
//         float ratio = mass / (character.mass + mass);
//         //PullPlayer(character, -force * ratio);
//         rigid_body.AddForce(force * (1 - ratio), ForceMode.Impulse);
//       } else if (hitBody != null) {
//         float ratio = mass / (hitBody.mass + mass);
//         hitBody.AddForce(-force * ratio, ForceMode.Impulse);
//         rigid_body.AddForce(force * (1 - ratio), ForceMode.Impulse);
//       } else {
//         rigid_body.AddForce(force, ForceMode.Impulse);
//         Vector3 grapple_direction = dist.normalized;
//         float grapple_distance = Vector3.Distance(hit_location.transform.position, head.position);
//         float grapple_speed = Mathf.Round(Vector3.Dot(rigid_body.velocity, grapple_direction) * 100) / 100;

//         if(grapple_speed < 0){
//           if(grapple_distance > initial_tongue_distance){
//             rigid_body.velocity -= grapple_speed * grapple_direction;
//             //rigid_body.position = hit_location.transform.position - (grapple_direction * initial_tongue_distance);
//           }
//         }
//       }
//       return(max_tongue_distance >= dist.magnitude);
//     }

//     // [Command (requiresAuthority = false)]
//     // void PullPlayer(Character character, Vector3 force) {
//     //   PullPlayerClient(character, force);
//     // }
//     // [ClientRpc]
//     // void PullPlayerClient(Character character, Vector3 force) {
//     //   character.spine.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
//     // }

//     public float getGravity() {
//         return gravity;
//     }

//     public void AimMarkerUpdate(){
//       if(Physics.Raycast(main_camera.position, main_camera.forward, out camera_hit, max_tongue_distance, is_grappleable) &&
//         Physics.Raycast(mouth.position, (camera_hit.point- mouth.position).normalized, out tongue_hit, max_tongue_distance, is_grappleable)) {
//         if(!aim_marker_mesh.enabled){
//           aim_marker_mesh.enabled = true;
//         }
//         aim_marker.transform.position = tongue_hit.point;
//         var dir = (aim_marker.transform.position - head.position).normalized;
//          ConfigurableJointExtensions.SetTargetRotationLocal(head_joint, Quaternion.LookRotation(dir), head_initial_pos);
//       } else if(aim_marker_mesh.enabled){
//         aim_marker_mesh.enabled = false;
//       }
//     }

//     private void OnEnable()
//     {
//       // max_tongue_distance = GameGlobals.globals.max_tongue_distance;
//       // jump_arc = GetComponent<LineRenderer>();
//       // collision = GetComponent<PlayerCollision>();
//       // aim_marker = Instantiate(aim_marker_prefab) as GameObject;
//       // aim_marker_mesh = aim_marker.GetComponent<MeshRenderer>();
//       // jump_arc.enabled = false;
//       // aim_marker_mesh.enabled = false;
//       // head_initial_pos = head.localRotation;
//       // spine_initial_pos = new Quaternion(0f, 0f, 0f, 1);
//       // spine = GetComponentInChildren<Impact>();


//       // if(this.isLocalPlayer) {
//       //   main_camera.gameObject.SetActive(true);
//       //   Cursor.lockState = CursorLockMode.Locked;
//       //   movement_machine = new StateMachine();
//       //   standing_state = new  StandingState(this,movement_machine);
//       //   jumping_state = new JumpingState(this,movement_machine);
//       //   falling_state = new FallingState(this,movement_machine);
//       //   swinging_state = new SwingingState(this,movement_machine);

//       //   movement_machine.Initialize(standing_state);

//       //   action_machine = new StateMachine();

//       //   idle_state = new IdleState(this, action_machine);
//       //   aiming_state = new AimingState(this, action_machine);
//       //   grappling_state = new GrappleState(this, action_machine);

//       //   action_machine.Initialize(idle_state);
//       //   rigid_body.isKinematic = false;
//       //   standing_cooldown = gameObject.AddComponent<Timer>();
//       //   standing_cooldown.SetTimer(.5f);
//       //   GetComponentInChildren<Impact>().enabled = true;
//       //   foreach(Transform transform in GetComponentsInChildren<Transform>()) { // this is so the player can't grapple to themselves
//       //     transform.gameObject.layer = 9;
//       //   }
//       //}


//     }

//     // [Command (requiresAuthority = false)]
//     // public void AddShockWave(float force, Vector3 source, float radius, Quaternion rotation) {
//     //   AddShockWaveClient(force, source, radius, rotation);
//     // }
//     // [ClientRpc]
//     // public void AddShockWaveClient(float force, Vector3 source, float radius, Quaternion rotation) {
//     //   explosionEffect.startLifetime = radius / 100;
//     //   Instantiate(explosionEffect, source, rotation);
//     //   rigid_body.AddExplosionForce(force, source, radius);
//     // }

//     // private void Update()
//     // {
//     //   if(this.isLocalPlayer) {
//     //     movement_machine.cur_state.HandleInput();
//     //     action_machine.cur_state.HandleInput();
//     //     action_machine.cur_state.LogicUpdate();
//     //     movement_machine.cur_state.LogicUpdate();
//     //   }
//     // }

//     // private void FixedUpdate() 
//     // {
//     //   if (this.isLocalPlayer) {
//     //     movement_machine.cur_state.PhysicsUpdate();
//     //     action_machine.cur_state.PhysicsUpdate();
//     //   }
//     // }

//     // public void ResetCharacter() {
//     //     main_camera.gameObject.SetActive(false);
//     //     rigid_body.isKinematic = true;
//     //     spine.enabled = false;
//     //     spine.transform.position = new Vector3(0, 0, 0);
//     //     GetComponent<Character>().enabled = false;
//     // }
// }


