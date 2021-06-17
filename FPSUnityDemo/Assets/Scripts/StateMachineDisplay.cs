using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateMachineDisplay : MonoBehaviour
{
    [SerializeField]
    private StateMachine state = null;
    private Text cur_text;
    
    public Character character = null;
    void Start()
    {
        if(character != null){
            state = character.movement_machine;
        }
    }
    public void SetState(StateMachine new_State){
        state = new_State;
    }

    // Update is called once per frame
    void Update()
    {
        // if(state != null){
        //     switch(state.cur_state){
        //         case RunningState:
        //             cur_text.text = "Running";
        //             break;
        //         case SlidingState:
        //             cur_text.text = "Sliding";
        //             break;
        //         case FallingState:
        //             cur_text.text = "Falling";
        //             break;
        //         case WallClimbingState:
        //             cur_text.text = "Climbing";
        //             break;
        //         case WallRunningState:
        //             cur_text.text = "Wall Run";
        //             break;
        //     }
        // }
    }
}
