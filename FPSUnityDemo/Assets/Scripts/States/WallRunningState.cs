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
        MonoBehaviour.print(character.GetWallDifference());
    }
    public override void Exit()
    {
        base.Exit();
        character.wallRunDurationTimer.StopTimer();
        character.EndWallRun();
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(character.controller.isGrounded){
            MonoBehaviour.print("hit ground");
            state_machine.ChangeState(character.running_state);
        } else if(!character.wallRunDurationTimer.is_active){
            MonoBehaviour.print("timer ended");
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        } else if(character.can_jump){
            MonoBehaviour.print("jump");
            //character.can_wall_run = false; 
            character.WallJump();
            state_machine.ChangeState(character.falling_state);
        }else if(!character.character_collisions.on_wall ||character.input_handler.move_input.z < .5f){
            MonoBehaviour.print("collisions: " + !character.character_collisions.on_wall + " input: " + (character.input_handler.move_input.z < .5f));
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