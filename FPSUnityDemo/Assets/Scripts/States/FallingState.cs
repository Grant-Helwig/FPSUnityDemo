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
        character.SetDebugText("Falling");
        float horizontal_velocity = new Vector3(character.velocity.x, 0 , character.velocity.z).magnitude;
        character.SetAirValues(horizontal_velocity);
        character.SetAnimation(Anim.Falling);
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
        //still jump if coyote time is active
        if(character.coyoteTimer.is_active && character.can_jump){
            character.GroundJump();
        
        //if you are on the ground and didnt just jump, go the running / sliding states
        } else if(character.character_collisions.on_ground && !character.jumpCooldownTimer.is_active){
            if(character.input_handler.is_sliding){
                character.SetSlideValues();
                state_machine.ChangeState(character.sliding_state);
            } else {
                state_machine.ChangeState(character.running_state);
            }
        //transition to wall states 
        } else if(character.character_collisions.on_wall){
            //can wall run is reset when touching the ground, always allow wall states after this
            if(character.GetWallDifference() <= .7f  && character.can_wall_run && !character.wallJumpCooldownTimer.is_active){
                character.SetWallRunValues();
                if(character.character_collisions.facing_wall){
                    state_machine.ChangeState(character.wall_climbing_state);
                } else {
                    state_machine.ChangeState(character.wall_running_state);
                }
            //otherwise if you jumped off of a wall previously, allow the character to go to another wall
            //timer check is so the player can jump without snapping back to the wall 
            } 
            // else if(!character.jumpCooldownTimer.is_active 
            //     && (character.movement_machine.prev_state == character.wall_running_state 
            //     || character.movement_machine.prev_state == character.wall_climbing_state)){
            //     if(character.character_collisions.facing_wall){
            //         MonoBehaviour.print("enter climbing state 2");
            //         state_machine.ChangeState(character.wall_climbing_state);
            //     } else {
            //         state_machine.ChangeState(character.wall_running_state);
            //     }
            // }
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