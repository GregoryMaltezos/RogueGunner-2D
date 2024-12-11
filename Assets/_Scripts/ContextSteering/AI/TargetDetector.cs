using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : Detector
{
    [SerializeField]
    private float targetDetectionRange = 5;

    [SerializeField]
    private LayerMask obstaclesLayerMask, playerLayerMask;

    [SerializeField]
    private bool showGizmos = false;

    // Gizmo parameters
    private List<Transform> colliders;


    /// <summary>
    /// Detects the player within a specified range and determines if the player is visible, 
    /// considering potential obstacles in the way.
    /// </summary>
    /// <param name="aiData">The AI data where detected targets will be stored.</param>
    public override void Detect(AIData aiData)
    {
        // Find out if player is near
        Collider2D playerCollider =
            Physics2D.OverlapCircle(transform.position, targetDetectionRange, playerLayerMask);

        if (playerCollider != null)
        {
            // Calculate the direction to the player
            Vector2 direction = (playerCollider.transform.position - transform.position).normalized;
            // Cast a ray to detect obstacles in the path to the player
            RaycastHit2D hit =
                Physics2D.Raycast(transform.position, direction, targetDetectionRange, obstaclesLayerMask);

            // Confirm that the detected collider belongs to the player layer
            if (hit.collider != null && (playerLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.DrawRay(transform.position, direction * targetDetectionRange, Color.magenta);
                colliders = new List<Transform>() { playerCollider.transform };  // Add the player's transform to the colliders list
            }
            else
            {
                // Player is not visible due to an obstacle
                colliders = null;
            }
        }
        else
        {
            // No player detected within the range
            colliders = null;
        }
        aiData.targets = colliders;  // Update the AI data with the detected targets
    }

    /// <summary>
    /// Draws gizmos in the Scene view to visualize the detection range and detected targets.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (showGizmos == false)
            return;

        Gizmos.DrawWireSphere(transform.position, targetDetectionRange);

        if (colliders == null)
            return;
        Gizmos.color = Color.magenta;
        foreach (var item in colliders)
        {
            Gizmos.DrawSphere(item.position, 0.3f);
        }
    }
}
