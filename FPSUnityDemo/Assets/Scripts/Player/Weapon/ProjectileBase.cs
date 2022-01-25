using UnityEngine;
using UnityEngine.Events;

public class ProjectileBase : MonoBehaviour
{
    public GameObject owner { get; private set; }
    public Vector3 initialPosition { get; private set; }
    public Vector3 initialDirection { get; private set; }
    public Vector3 shootOffset { get; private set; }
    public Vector3 inheritedMuzzleVelocity { get; private set; }
    public float initialCharge { get; private set; }
    public Camera weaponCamera { get; private set; }
    public Transform lookAt { get; private set; }

    public UnityAction onShoot;

    public void Shoot(BlowDart controller)
    {
        owner = controller.owner;
        initialPosition = transform.position;
        initialDirection = transform.forward;
        inheritedMuzzleVelocity = controller.muzzleWorldVelocity;
        initialCharge = controller.currentCharge;
        weaponCamera = controller.weaponCamera;
        lookAt = controller.lookAt;
        shootOffset = controller.shotDirection;

        if (onShoot != null)
        {
            onShoot.Invoke();
        }
    }
}
