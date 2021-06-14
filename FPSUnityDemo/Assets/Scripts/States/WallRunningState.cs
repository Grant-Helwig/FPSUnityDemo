using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningState : State
{
    public WallRunningState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.SetWallRunValues();
    }
    public override void Exit()
    {
        base.Exit();
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
        } else if(character.can_jump){
            //character.can_wall_run = false; 
            character.WallJump();
            state_machine.ChangeState(character.falling_state);
        }else if(!character.character_collisions.on_wall ||character.input_handler.move_input.z < .5f){
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        } 
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if(character.current_camera_roll != character.max_angle_roll){
            character.SetCameraAngle(-character.wall_direction * character.max_angle_roll);
        }
        character.WallRun();
    }
}