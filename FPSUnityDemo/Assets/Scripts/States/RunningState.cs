using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningState : State
{
    public RunningState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.lastWallNormal = Vector3.zero;
        character.can_wall_run = true;
        character.SnapToGround();
        character.SetDebugText("Running");
        character.SetAnimation(Anim.Running);
        character.SetAnimationThirdPerson(Anim.Running);
        character.thirdPersonIK.enabled = true;
    }
    public override void Exit()
    {
        base.Exit();
        character.thirdPersonIK.enabled = false;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        // if(Input.GetButtonDown("Slide")){
        // character.SetSlideValues();
        // }
    }
    public override void LogicUpdate()
    {
        if(character.velocity.magnitude < 2f && character.curAnimState != Anim.Idle){
            character.SetAnimation(Anim.Idle);
            character.SetAnimationThirdPerson(Anim.Idle);
        } else if(character.velocity.magnitude >= 2f && character.curAnimState != Anim.Running) {
            character.SetAnimation(Anim.Running);
            character.SetAnimationThirdPerson(Anim.Running);
        }
        base.LogicUpdate();
        if(!character.character_collisions.on_ground){
            character.coyoteTimer.StartTimer();
            state_machine.ChangeState(character.falling_state);
        } else if(character.can_jump){
            if(character.character_collisions.on_wall){
                character.GroundJump();
                state_machine.ChangeState(character.falling_state);
            } else {
                character.GroundJump();
                state_machine.ChangeState(character.falling_state);
            }
        } else if(character.input_handler.is_sliding){
            character.SetSlideValues();
            if(character.CrouchThreshold()){
                state_machine.ChangeState(character.sliding_state);
            } else {
                state_machine.ChangeState(character.sliding_state);
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

        character.GroundMovement();
    }
}
