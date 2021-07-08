using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterCollisions : MonoBehaviour
{
    public bool on_wall;
    public bool facing_wall;
    public bool on_ground; 
    public bool on_ceiling; 
    public float wall_angle;
    public Vector3 ground_slope; 
    public Vector3 last_wall_position;
    public Vector3 last_wall_normal;
    public Vector3 last_facing_wall_position;
    public Vector3 last_facing_wall_normal;
    public LayerMask mask;
    Vector3[] wall_directions;
    Vector3[] facing_wall_directions;
    RaycastHit[] wall_hits;
    RaycastHit[] facing_wall_hits;
    public RaycastHit ground_hit;
    [SerializeField]
    private float max_wall_dist;
    [SerializeField]
    private float max_facing_wall_dist;
    private CharacterController controller;
    private float ground_check_dist = .5f;

    void Start()
    {
        wall_directions = new Vector3[]{ 
            Vector3.right, 
            Vector3.right + Vector3.forward,
            Vector3.right + Vector3.back,
            Vector3.forward, 
            Vector3.left + Vector3.forward, 
            Vector3.left + Vector3.back, 
            Vector3.left,
            Vector3.back
        };
        facing_wall_directions = new Vector3[]{ 
            Vector3.right + Vector3.forward,
            Vector3.forward, 
            Vector3.left + Vector3.forward
        };
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    private void Update() {
        on_wall = WallCheck();
        on_ground = GroundCheck();
        on_ceiling = CeilingCheck();
        //facing_wall = FacingWallCheck();
    }

    public Vector3 WallHitNormal(){
        return last_wall_normal;
    }

    public bool WallCheck(){
        wall_hits = new RaycastHit[wall_directions.Length];
        Vector3 lower_body_position = new Vector3(transform.position.x, transform.position.y + (controller.height / 4), transform.position.z);
        for(int i=0; i<wall_directions.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(wall_directions[i]);
            Physics.Raycast(lower_body_position, dir, out wall_hits[i], max_wall_dist, mask);
            if(wall_hits[i].collider != null)
            {
                Debug.DrawRay(lower_body_position, dir * wall_hits[i].distance, Color.green);
            }
            else
            {
                Debug.DrawRay(lower_body_position, dir * max_wall_dist, Color.red);
            }
        }
        
        wall_hits = wall_hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
        if(wall_hits.Length > 0)
        {
            last_wall_position = wall_hits[0].point;
            last_wall_normal = wall_hits[0].normal;

            Vector3 along_wall = transform.TransformDirection(Vector3.forward);
            Vector3 orthogonal_wall_vector = Vector3.Cross(last_wall_normal, Vector3.up);
            wall_angle = Vector3.Dot(along_wall, last_wall_normal);
            facing_wall = wall_angle < -.7f ? true : false;

            return true && !SlopeLimitCheck(last_wall_normal);
        }  else {
            last_wall_normal = Vector3.zero;
            return false;
        }
    }
    public bool FacingWallCheck(){
        facing_wall_hits = new RaycastHit[facing_wall_directions.Length];
        Vector3 middle_body_position = new Vector3(transform.position.x, transform.position.y + (controller.height / 2), transform.position.z);
        
        for(int i=0; i<facing_wall_directions.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(facing_wall_directions[i]);
            Physics.Raycast(middle_body_position, dir, out facing_wall_hits[i], max_facing_wall_dist);
            if(facing_wall_hits[i].collider != null)
            {
                Debug.DrawRay(middle_body_position, dir * facing_wall_hits[i].distance, Color.green);
            }
            else
            {
                Debug.DrawRay(middle_body_position, dir * max_facing_wall_dist, Color.red);
            }
        }
        
        facing_wall_hits = facing_wall_hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
        if(wall_hits.Length == facing_wall_directions.Length)
        {
            last_facing_wall_position = wall_hits[0].point;
            last_facing_wall_normal = wall_hits[0].normal;
            return true;
        } else {
            return false;
        }
    }

    public bool GroundCheck(){
        float chosen_ground_check_dist = on_ground ? (controller.skinWidth + ground_check_dist) : .05f;
        if(Physics.CapsuleCast(BottomHemisphere(), TopHemisphere(controller.height), controller.radius, Vector3.down, out ground_hit, chosen_ground_check_dist, mask)){
            ground_slope = ground_hit.normal;
            if (Vector3.Dot(ground_slope, transform.up) > 0f && SlopeLimitCheck(ground_slope)){
                return true;
            }
        }
        return false;
    }

    public bool CeilingCheck(){
        float ceiling_check_dist = (controller.skinWidth + .5f);
        if(Physics.CapsuleCast(BottomHemisphere(), TopHemisphere(controller.height), controller.radius, Vector3.up, out ground_hit, ceiling_check_dist, mask)){
            return true;
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
