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
        }
    }
    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(!character.input_handler.is_grappling){
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
        character.GrappleMovement();
    }
}