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
        if(character.controller.isGrounded){
            state_machine.ChangeState(character.running_state);
        } else if(character.character_collisions.on_wall && character.can_wall_run){
            state_machine.ChangeState(character.wall_running_state);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        character.AirMovement();
    }
}