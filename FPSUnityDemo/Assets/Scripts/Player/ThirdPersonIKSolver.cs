using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonIKSolver : MonoBehaviour
{
    [SerializeField] float speed = 1;
    [SerializeField] float stepDistance = 4;
    [SerializeField] float stepLength = 4;
    [SerializeField] float stepHeight = 1;
    [SerializeField] Vector3 footOffset = default;
    [SerializeField] ThirdPersonIKSolver otherFoot = default;
    [SerializeField] Transform body = default;
    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Character character;
    private float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    private float footLerp;
    int direction;
    void Start()
    {
        //initialize all IK values
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;
        footLerp = 1;
    }

    void Update()
    {
        //set the IK object to the current target position
        transform.position = currentPosition;
        
        //transform.forward = currentNormal;//new Vector3(currentNormal.x,-body.up.y ,currentNormal.z); //currentNormal + body.forward;
        //transform.RotateAround(transform.position, Vector3.up,Vector3.Angle(transform.up, -body.up) );

        //make a ray that goes from the side of the body to the ground
        Vector3 offset = body.position;
        Vector3 footVector;
        Ray ray;
        if(character.input_direction.magnitude > .1){
            footVector = Vector3.Cross(character.input_direction, Vector3.up).normalized;
            offset = Vector3.MoveTowards(offset, offset + (-footVector * footSpacing) + (character.input_direction * stepDistance / 2), stepDistance / 2);
            ray = new Ray(offset, Vector3.down);
        } else {
            footVector = Vector3.Cross(character.input_direction, Vector3.up).normalized;
            offset = Vector3.MoveTowards(offset, offset + (character.transform.right * footSpacing), stepDistance / 2);
            ray = new Ray(offset, Vector3.down);
            // footVector = Vector3.Cross(character.input_direction, Vector3.up).normalized;
            // offset = Vector3.MoveTowards(offset, offset + (-footVector * footSpacing) + (character.input_direction * stepDistance / 2), stepDistance / 2);
            // ray = new Ray(offset, Vector3.down);
        }
        //Vector3 footVector = Vector3.Cross(character.input_direction, Vector3.up).normalized;
        //offset = Vector3.MoveTowards(offset, offset + (-footVector * footSpacing) + (character.input_direction * stepDistance / 2), stepDistance / 2);
        //Ray ray = new Ray(offset, Vector3.down);
        //get the ray info
        if (Physics.Raycast(ray, out RaycastHit info, 10, terrainLayer.value))
        {
            //print(info.collider.name);
            //check if the ray position is great enough to make a step. This foot should be moving and the other foot should not be
            if ((Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && footLerp >= 1))
            {
                //set local foot lerp to 0, so now it is the one that is moving 
                footLerp = 0;

                //make sure the foot is moving in the right direction
                //direction = body.InverseTransformPoint(info.point).y < body.InverseTransformPoint(newPosition).y ? 1 : -1;
                
                //new position is the ray point under you going forward a step
                //newPosition = Vector3.MoveTowards(newPosition, info.point + (character.input_direction * (stepLength / 2) * direction) + footOffset, stepLength / 2);
                newPosition = info.point + (character.input_direction * stepLength * direction);
                
                //make it so the foot is angled to the floor
                newNormal = info.normal;
            }
        }

        if (footLerp < 1)
        {   
            //newPosition = Vector3.MoveTowards(newPosition, info.point + (character.input_direction * (stepLength / 2) * direction) + footOffset, stepLength / 2);
            newPosition = info.point + (character.input_direction * stepLength * direction);
            newNormal = info.normal;
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, footLerp);
            tempPosition.y += Mathf.Sin(footLerp * Mathf.PI) * stepHeight;

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, footLerp);
            footLerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;
        }
    }
    public bool IsMoving()
    {
        return footLerp < 1;
    }
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.2f);
        Vector3 offset = body.position;
        Vector3 footVector = Vector3.Cross(character.input_direction, Vector3.up).normalized;
        offset = Vector3.MoveTowards(offset, offset + (-footVector * footSpacing) + (character.input_direction * stepDistance / 2), stepDistance / 2);
        Gizmos.DrawSphere(offset, 0.2f);
        //Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
