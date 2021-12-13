using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbGravity : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody r;
    void Start()
    {
        r = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        r.AddForce(Vector3.down * 20f * Time.fixedDeltaTime, ForceMode.Force);
    }
}
