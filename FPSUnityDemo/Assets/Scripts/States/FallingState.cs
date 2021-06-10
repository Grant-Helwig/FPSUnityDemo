using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingState : State
{
    public FallingState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
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
        if(character.coyote_timer.is_active && character.can_jump){
            character.GroundJump();
        } else if(character.character_collisions.on_ground && !character.jump_cooldown_timer.is_active){
            if(character.input_handler.is_sliding){
                if(character.CrouchThreshold()){
                    state_machine.ChangeState(character.sliding_state);
                } else {
                    state_machine.ChangeState(character.sliding_state);
                }
            } else {
                state_machine.ChangeState(character.running_state);
            }
        } else if(character.character_collisions.on_wall && character.can_wall_run ){
            if(character.can_wall_run){
                state_machine.ChangeState(character.wall_running_state);
            } else if(!character.jump_cooldown_timer.is_active && character.movement_machine.prev_state == character.wall_running_state){
                state_machine.ChangeState(character.wall_running_state);
            }
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
        character.AirMovement();
    }
}