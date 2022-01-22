using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision 
{
    public bool on_ground;
    public bool on_ceiling;
    public LayerMask ground_layer;
    private float distance_to_ground;
    public SphereCollider player_head;
    public Transform player_position;
    public bool sphere_collided;
    public Vector3 sphere_collision_angle;
    public int sphere_array_size;
    public float sphere_array_length;
    private Vector3[] sphere_ray_points;
    private RaycastHitInfo[] rays;
    public RaycastHitInfo closest_hit;

    public class RaycastHitInfo
     {
         public RaycastHit ray;
         public Vector3 direction;
         public float currentHitDistance;
         public RaycastHitInfo(RaycastHit hit, Vector3 dir, float curDist)
         {
             ray = hit;
             direction = dir;
             currentHitDistance = curDist;
         }
     }

    void Start()
    {
        distance_to_ground = player_head.radius * 12;
        sphere_ray_points = GetPointsOnSphere(sphere_array_size);
        rays = new RaycastHitInfo[sphere_array_size];

        for (int i=0; i < sphere_array_size; i++)
         {
             rays[i] = new RaycastHitInfo(default(RaycastHit), default(Vector3), default(float));
         }
    }

    void Update()
    {
    //   if (this.isLocalPlayer) {
    //     if(Physics.Raycast(player_head.gameObject.transform.position,Vector3.down,distance_to_ground + .1f, ground_layer)){
    //         on_ground = true;
    //     } else {
    //         on_ground = false;
    //     }

    //     if(Physics.Raycast(player_head.gameObject.transform.position,Vector3.up,distance_to_ground + .1f)){
    //         on_ceiling = true;
    //     } else {
    //         on_ceiling = false;
    //     }
        
    //     closest_hit = GetClosestPointOnSphere();
    //     if(closest_hit.currentHitDistance < sphere_array_length){
    //         sphere_collided = true;
    //     } else {
    //         sphere_collided = false;
    //     }
    //   }
    }

    void OnDrawGizmos()
    {
        //Debug.DrawRay((Vector3)transform.position, Vector3.down * distance_to_ground);
        //Debug.DrawRay((Vector3)transform.position, Vector3.up * distance_to_ground);

        if(sphere_collided){
            Gizmos.DrawRay(player_position.position, Vector3.Cross(closest_hit.direction, Vector3.up) * closest_hit.currentHitDistance);
        }
    }

    private Vector3[] GetPointsOnSphere(int nPoints)
    {
        float fPoints = (float)nPoints;
 
        Vector3[] points = new Vector3[nPoints];
 
        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2 / fPoints;
 
        for (int k = 0; k < nPoints; k++)
        {
            float y = k * off - 1 + (off / 2);
            float r = Mathf.Sqrt(1 - y * y);
            float phi = k * inc;
 
            points[k] = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
        }
 
        return points;
    }

    private RaycastHitInfo GetClosestPointOnSphere(){
        RaycastHit hit;
        for(int i = 0; i < sphere_array_size; i++){
            if(Physics.Raycast(player_position.position,sphere_ray_points[i], out hit, sphere_array_length, ground_layer)){
                rays[i].ray = hit;
                rays[i].direction = sphere_ray_points[i];
                rays[i].currentHitDistance = hit.distance;
            } else {
                rays[i].ray = hit;
                rays[i].direction = sphere_ray_points[i];
                rays[i].currentHitDistance = sphere_array_length;
            }
        }

        float min = sphere_array_length + 1;
        int closest_point_index = 0;
        for(int i = 0; i < sphere_array_size; i++){
            if(rays[i].currentHitDistance < min){
                min = rays[i].currentHitDistance;
                closest_point_index = i;
            }
        } 

        return rays[closest_point_index];
    }
}