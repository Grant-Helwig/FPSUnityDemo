using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float wait_time { get; private set; }
    public bool is_active { get; private set; }
    public float cur_time { get; private set; }
    private Coroutine wait_routine;
    private float time_counter;
    
    public void SetTimer(float wait_time){
        this.wait_time = wait_time; 
    }
    
    IEnumerator TimerCoroutine()
    {
        is_active = true;
        yield return new WaitForSeconds(wait_time);
        is_active = false; 
    }

    public void StartTimer(){
        if(!is_active){
            cur_time = 0f;
            time_counter = 0f;
            wait_routine = StartCoroutine(TimerCoroutine());
        }
    }

    public void StopTimer(){
        if(wait_routine != null){
            cur_time = 0f;
            time_counter = 0f;
            StopCoroutine(wait_routine);
            is_active = false;
        }
    }
    private void Update() {
        if(is_active){
            time_counter += Time.deltaTime;
            cur_time = Mathf.Lerp(0,wait_time, time_counter / wait_time);
        }
    }
}
