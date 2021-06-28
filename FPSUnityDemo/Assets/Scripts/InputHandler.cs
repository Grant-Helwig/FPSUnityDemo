using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public bool is_jumping = false;
    public bool is_sliding;
    public bool is_sprinting;
    public bool is_grappling;
    public Vector3 move_input;
    public Vector2 mouse_delta;
    // Start is called before the first frame update
    void Start()
    {
        is_grappling = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnJump(InputValue value){
        print("jump pressed");
        is_jumping = value.isPressed;
    }

    void OnSlide(InputValue value){
        is_sliding = value.isPressed;
    }
    void OnSprint(InputValue value){
        is_sprinting = value.isPressed;
    }

    void OnMove(InputValue value){
        move_input.x = value.Get<Vector2>().x;
        move_input.z = value.Get<Vector2>().y;
    }
    void OnLook(InputValue value){
        mouse_delta = value.Get<Vector2>();
        // Account for scaling applied directly in Windows code by old input system.
        mouse_delta *= 0.5f;
        
        // Account for sensitivity setting on old Mouse X and Y axes.
        mouse_delta *= 0.1f;
    }
}
