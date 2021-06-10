using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float wait_time { get; private set; }
    public bool is_active { get; private set; }
    private Coroutine wait_routine;
    
    public void SetTimer(float wait_time){
        this.wait_time = wait_time; 
    }
    // public Timer(float wait_time){
    //     this.wait_time = wait_time;
    // }
    
    IEnumerator TimerCoroutine()
    {
        is_active = true;
        yield return new WaitForSeconds(wait_time);
        is_active = false; 
    }

    public void Start(){
        if(!is_active){
            wait_routine = StartCoroutine(TimerCoroutine());
        }
    }

    public void Stop(){
        if(wait_routine != null){
            StopCoroutine(wait_routine);
            is_active = false;
        }
    }
}
