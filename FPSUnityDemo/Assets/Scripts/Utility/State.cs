using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    protected Character character; // replace with your own controller
    protected StateMachine state_machine; 

    protected State(Character character, StateMachine state_machine){
        this.character = character;
        this.state_machine = state_machine;
    }
    public virtual void Enter(){
    }

    public virtual void HandleInput(){
    }

    public virtual void LogicUpdate(){
    }

    public virtual void PhysicsUpdate(){
    }

    public virtual void Exit(){
    }
}
