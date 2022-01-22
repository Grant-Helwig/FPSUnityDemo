using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootState : State
{
    public ShootState(Character character, StateMachine stateMachine) : base(character, stateMachine){

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
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
} 