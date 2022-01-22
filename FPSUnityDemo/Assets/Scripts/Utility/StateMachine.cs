using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine
{
    public State cur_state { get; private set; }
    public State prev_state { get; private set; }


    public void Initialize(State starting_state){
        cur_state = starting_state;
        cur_state.Enter();
    }

    public void ChangeState(State new_state){
        cur_state.Exit();
        prev_state = cur_state;
        cur_state = new_state;
        cur_state.Enter();
    }
}