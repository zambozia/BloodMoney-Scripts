using Gley.PedestrianSystem.Internal;
using System.Diagnostics;
using UnityEngine;

public class RagdollColliderEnabler : MonoBehaviour
{
    [Tooltip("Assign the colliders you want active during gameplay for collision detection.")]
    public Collider[] collidersToEnable;

    void Start()
    {
        StartCoroutine(ReEnableColliders());
    }

    System.Collections.IEnumerator ReEnableColliders()
    {
        yield return new WaitForSeconds(1f);

        foreach (var col in collidersToEnable)
        {
            if (col != null)
            {
                col.enabled = true;

                var rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        UnityEngine.Debug.Log($"Player collided with {collision.gameObject.name}", this);

        var pedestrian = collision.collider.GetComponentInParent<Pedestrian>();
        if (pedestrian != null)
        {
            UnityEngine.Debug.Log($"Forwarding collision to Gley for: {pedestrian.name}", pedestrian);

            // Force Gley's internal collision response
            Events.OnCollisionEnter?.Invoke(pedestrian.Index, collision, ObstacleTypes.Car);
        }
    }
}
