using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallClimbingState : State
{
    public WallClimbingState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.SetWallClimbValues();
    }
    public override void Exit()
    {
        base.Exit();
        character.wall_climb_duration_timer.Stop();
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
        } else if(!character.wall_climb_duration_timer.is_active){
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        } else if(character.can_jump){
            //character.can_wall_run = false; 
            character.WallJump();
            state_machine.ChangeState(character.falling_state);
        }else if(!character.character_collisions.facing_wall ||character.input_handler.move_input.z < .5f){
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if(!character.AtHeight(character.standing_height)){
            character.SetCharacterHeight(false, character.standing_height);
        }
        if(character.current_camera_roll != 0){
            character.SetCameraAngle(0);
        }
        character.WallClimb();
    }
}