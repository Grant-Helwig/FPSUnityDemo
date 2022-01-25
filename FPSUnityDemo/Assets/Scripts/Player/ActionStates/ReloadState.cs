using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : State
{
    public ReloadState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.SetDebugTextAction( "Reload");
    }
    public override void Exit()
    {
        base.Exit();
        character.blowDart.ResetAmmoCounter();
    }
    public override void HandleInput()
    {
        base.HandleInput();
        if(!character.input_handler.is_reloading){
            state_machine.ChangeState(character.idle_state);
        }
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(character.blowDart.UpdateAmmo()){
            state_machine.ChangeState(character.idle_state);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}