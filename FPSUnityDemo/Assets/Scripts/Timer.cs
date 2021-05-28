using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float wait_time { get; private set; }
    public bool is_active { get; private set; }
    
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
            StartCoroutine(TimerCoroutine());
        }
    }
    void Start()
    {
        is_active = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
