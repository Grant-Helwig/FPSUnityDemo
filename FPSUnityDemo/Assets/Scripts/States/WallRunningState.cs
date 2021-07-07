using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningState : State
{
    public WallRunningState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }
    private float last_cam_angle;
    public override void Enter()
    {
        base.Enter();
        character.SetDebugText( "Wall Run");
        character.SetAnimation(Anim.Running);
        character.EnableWallRunArm();
    }
    public override void Exit()
    {
        base.Exit();
        character.wallRunDurationTimer.StopTimer();
        character.EndWallRun();
        character.DisableWallRunArms();
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if(character.controller.isGrounded){
            state_machine.ChangeState(character.running_state);
        } else if(!character.wallRunDurationTimer.is_active){
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        } else if(character.can_jump){
            character.WallJump();
            state_machine.ChangeState(character.falling_state);
        }else if(!character.character_collisions.on_wall || Vector3.Dot(character.input_direction, character.character_collisions.last_wall_normal) > .6){//character.input_handler.move_input.z < .5f){ // change this to be slightly towards wall instead of just relative forward 
            //MonoBehaviour.print("not aiming towards wall : " + character.character_collisions.wall_hits[0].collider.gameObject.name); // Vector3.Dot(character.input_direction, character.character_collisions.last_wall_normal));
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        } else if(character.character_collisions.wall_angle < -.6 
        && Mathf.Sign(character.cur_wall_direction) != Mathf.Sign(character.wall_direction)){
            //MonoBehaviour.print("going wrong way :  current way" + character.character_collisions.wall_angle);
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float time_left = character.fixedAngleRollDuration - (character.wallRunDurationTimer.wait_time - character.wallRunDurationTimer.cur_time);
        if(time_left >= 0){
            character.SetCameraAngleFixed(0, last_cam_angle, Mathf.Pow(time_left / character.fixedAngleRollDuration, 2f));
        } else if(character.current_camera_roll != character.max_angle_roll){
            last_cam_angle = character.SetCameraAngle(-character.wall_direction * character.max_angle_roll);
        }
        character.WallRun();
    }

    
}