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
        } else if(!character.character_collisions.on_wall || Input.GetAxisRaw("Vertical") <= 0){
            character.can_wall_run = false; 
            state_machine.ChangeState(character.falling_state);
        } 
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        character.WallRun();
    }
}