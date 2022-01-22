using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
public class SlidingState : State
{
    float target_velocity_speed;
    public SlidingState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.can_wall_run = true;
        character.lastWallNormal = Vector3.zero;
        character.ResetSlideTimer();
        character.SnapToGround();
        character.SetDebugText("Sliding");
        character.SetAnimation(Anim.Sliding);
        //character.CrouchEffector(true);
        //character.grounderBipedIK.enabled = false;
        //character.grounderBipedIK.solver.liftPelvisWeight = -1.5f;
        //character.grounderBipedIK.enabled = true;
        //character.hipIK.GetComponent<eff>
    }
    public override void Exit()
    {
        base.Exit();
        character.ResetSlideTimer();
        character.CrouchEffector(false);
        //character.grounderBipedIK.solver.liftPelvisWeight = .5f;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(!character.character_collisions.on_ground){
            state_machine.ChangeState(character.falling_state);
        } else if(!character.input_handler.is_sliding){
            state_machine.ChangeState(character.running_state);
        }else if(character.can_jump){
            character.GroundJump();
            state_machine.ChangeState(character.falling_state);
        }
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if(!character.AtHeight(character.crouching_height)){
            character.SetCharacterHeight(false, character.crouching_height);
        }
        if(character.current_camera_roll != 0){
            character.SetCameraAngle(0);
        }
        character.SlideMovement();
    }
}
