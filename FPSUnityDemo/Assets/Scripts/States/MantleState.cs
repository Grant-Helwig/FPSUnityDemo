using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantleState  : State
{
    Vector3 mantle_height;
    public MantleState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        mantle_height = character.character_collisions.mantle_height;
        character.SetDebugText( "Wall mantle");
        character.SetWallClimbValues();
        character.SetAnimation(Anim.Climbing);
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
        if(character.transform.position.y > mantle_height.y){
            state_machine.ChangeState(character.running_state);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        character.MantleMovement(mantle_height);
    }
}