using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterCollisions : MonoBehaviour
{
    public bool on_wall;
    public Vector3 last_wall_position;
    public Vector3 last_wall_normal;
    Vector3[] directions;
    RaycastHit[] hits;
    private Vector3 last_normal;
    [SerializeField]
    private float max_wall_dist;
    void Start()
    {
        directions = new Vector3[]{ 
            Vector3.right, 
            Vector3.right + Vector3.forward,
            //Vector3.right + Vector3.back,
            Vector3.forward, 
            Vector3.left + Vector3.forward, 
            //Vector3.left + Vector3.back, 
            Vector3.left//,
            //Vector3.back
        };
    }

    // Update is called once per frame
    void LateUpdate()
    {
        hits = new RaycastHit[directions.Length];

        for(int i=0; i<directions.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(directions[i]);
            Physics.Raycast(transform.position, dir, out hits[i], max_wall_dist);
            if(hits[i].collider != null)
            {
                Debug.DrawRay(transform.position, dir * hits[i].distance, Color.green);
            }
            else
            {
                Debug.DrawRay(transform.position, dir * max_wall_dist, Color.red);
            }
        }
        
        hits = hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
        if(hits.Length > 0)
        {
            on_wall = true;
            last_wall_position = hits[0].point;
            last_wall_normal = hits[0].normal;
            last_normal = hits[0].normal;
        } else {
            on_wall = false;
        }
        // for(int i=0; i<hits.Length; i++)
        // {
        //     print(hits[i].normal + " | " + i);
        // }
    }

    public Vector3 HitNormal(){
        return last_normal;
    }
}
