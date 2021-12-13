using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
public class RagdollState : State
{
    Timer ragdollTimer;
    public RagdollState(Character character, StateMachine stateMachine) : base(character, stateMachine){

    }

    public override void Enter()
    {
        base.Enter();
        character.thirdPersonIK.enabled = false;
        character.ragdoll.EnableRagdoll();
        character.hipIK.GetComponent<OffsetModifier>().enabled = true;
        character.SetDebugText( "Ragdoll");
        //character.ragdoll.RagdollEnableLimbs();
        ragdollTimer = character.gameObject.AddComponent<Timer>();
        ragdollTimer.SetTimer(5.0f);
        ragdollTimer.StartTimer();
    }
    public override void Exit()
    {
        base.Exit();
        character.ragdoll.DisableRagdoll();
        character.hipIK.GetComponent<OffsetModifier>().enabled = false;
        //character.ragdoll.RagdollDisableLimbs();
        character.thirdPersonIK.enabled = true;
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(!ragdollTimer.is_active){
            state_machine.ChangeState(character.falling_state);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        character.AirMovement();
    }
}