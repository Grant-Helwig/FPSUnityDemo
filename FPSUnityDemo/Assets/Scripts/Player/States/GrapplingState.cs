using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingState : State
{
    public GrapplingState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.SetDebugText( "Grappling");
        if(!character.StartGrapple()){
            state_machine.ChangeState(state_machine.prev_state);
        } else {
            character.lastWallNormal = Vector3.zero;
            character.can_wall_run = true;
            character.SetAnimation(Anim.Falling);
        }
    }
    public override void Exit()
    {
        base.Exit();
        character.tongue.SetActive(false);
        character.action_machine.ChangeState(character.idle_state);
    }
    public override void HandleInput()
    {
        base.HandleInput();
        if(character.stopGrapple || 
            character.GetGrappleDistance() < 5f ||
            character.GetGrappleAngle()< -.9f){
            if(character.character_collisions.on_ground && !character.jumpCooldownTimer.is_active){
                if(character.input_handler.is_sliding){
                    character.SetSlideValues();
                    state_machine.ChangeState(character.sliding_state);
                } else {
                    state_machine.ChangeState(character.running_state);
                }
            //transition to wall states 
            } else if(character.character_collisions.on_wall && !character.jumpCooldownTimer.is_active
            && Vector3.Dot(character.input_direction, character.character_collisions.last_wall_normal) <= .6
            && !Mathf.Approximately(character.input_direction.magnitude,0)){
                //can wall run is reset when touching the ground, always allow wall states after this
                if(character.GetWallDifference() <= .7f  && character.can_wall_run && !character.wallJumpCooldownTimer.is_active){
                    character.SetWallRunValues();
                    if(character.character_collisions.facing_wall){
                        state_machine.ChangeState(character.wall_climbing_state);
                    } else {
                        state_machine.ChangeState(character.wall_running_state);
                    }
                } 
        } else {
                state_machine.ChangeState(character.falling_state);
            }
            character.action_machine.ChangeState(character.idle_state);
        }
            
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        character.GrappleMovement();
        character.SetTongue();
    }
}