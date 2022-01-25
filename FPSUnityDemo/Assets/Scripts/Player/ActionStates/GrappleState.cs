using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleState : State
{
    public GrappleState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.DisbaleAimArm();
        character.SetDebugTextAction( "Grapple");
    }
    public override void Exit()
    {
        base.Exit();
        MonoBehaviour.print("exiting grapple");
        character.EnableAimArm();
    }
    public override void HandleInput()
    {
        base.HandleInput();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
} 