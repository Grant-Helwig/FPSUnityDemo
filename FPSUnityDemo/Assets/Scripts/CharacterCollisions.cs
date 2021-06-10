using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterCollisions : MonoBehaviour
{
    public bool on_wall;
    public bool on_ground; 
    public Vector3 ground_slope; 
    public Vector3 last_wall_position;
    public Vector3 last_wall_normal;
    Vector3[] wall_directions;
    RaycastHit[] wall_hits;
    RaycastHit ground_hit;
    [SerializeField]
    private float max_wall_dist;
    private CharacterController controller;
    private float ground_check_dist = .5f;

    void Start()
    {
        wall_directions = new Vector3[]{ 
            Vector3.right, 
            Vector3.right + Vector3.forward,
            //Vector3.right + Vector3.back,
            Vector3.forward, 
            Vector3.left + Vector3.forward, 
            //Vector3.left + Vector3.back, 
            Vector3.left//,
            //Vector3.back
        };
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        on_wall = WallCheck();
        on_ground = GroundCheck();
    }

    public Vector3 WallHitNormal(){
        return last_wall_normal;
    }

    public bool WallCheck(){
        wall_hits = new RaycastHit[wall_directions.Length];

        for(int i=0; i<wall_directions.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(wall_directions[i]);
            Physics.Raycast(transform.position, dir, out wall_hits[i], max_wall_dist);
            if(wall_hits[i].collider != null)
            {
                Debug.DrawRay(transform.position, dir * wall_hits[i].distance, Color.green);
            }
            else
            {
                Debug.DrawRay(transform.position, dir * max_wall_dist, Color.red);
            }
        }
        
        wall_hits = wall_hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
        if(wall_hits.Length > 0)
        {
            last_wall_position = wall_hits[0].point;
            last_wall_normal = wall_hits[0].normal;
            return true;
        } else {
            return false;
        }
    }

    public bool GroundCheck(){
        float chosen_ground_check_dist = on_ground ? (controller.skinWidth + ground_check_dist) : .05f;
        if(Physics.CapsuleCast(BottomHemisphere(), TopHemisphere(controller.height), controller.radius, Vector3.down, out ground_hit, chosen_ground_check_dist)){
            ground_slope = ground_hit.normal;
            if (Vector3.Dot(ground_slope, transform.up) > 0f && SlopeLimitCheck(ground_slope)){
                return true;
            }
        }
        return false;
    }

    public Vector3 BottomHemisphere(){
        return transform.position + (transform.up * controller.radius);
    }

    public Vector3 TopHemisphere(float height){
        return transform.position + (transform.up * (height - controller.radius));
    }

    public bool SlopeLimitCheck(Vector3 normal){
        return Vector3.Angle(transform.up, normal) <= controller.slopeLimit;
    }
}
