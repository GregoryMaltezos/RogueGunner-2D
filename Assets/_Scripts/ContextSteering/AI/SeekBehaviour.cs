using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeekBehaviour : SteeringBehaviour
{
    [SerializeField]
    private float targetRechedThreshold = 0.5f;

    [SerializeField]
    private bool showGizmo = true;

    bool reachedLastTarget = true;

    //gizmo parameters
    private Vector2 targetPositionCached;
    private float[] interestsTemp;


    /// <summary>
    /// Calculates the steering behaviors for seeking a target.
    /// </summary>
    /// <param name="danger">Array of danger values indicating the level of danger in each direction (unused in this method but passed from the base class).</param>
    /// <param name="interest">Array of interest values that indicate the desired directions for the agent to move towards.</param>
    /// <param name="aiData">AI data that contains information about the current target and other relevant data.</param>
    /// <returns>Updated danger and interest arrays.</returns>
    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        //if we don't have a target stop seeking
        //else set a new target
        if (reachedLastTarget)
        {
            if (aiData.targets == null || aiData.targets.Count <= 0)
            {
                aiData.currentTarget = null;
                return (danger, interest); // No targets to seek
            }
            else
            {
                // Select the nearest target from the list of available targets
                reachedLastTarget = false;
                aiData.currentTarget = aiData.targets.OrderBy
                    (target => Vector2.Distance(target.position, transform.position)).FirstOrDefault();
            }

        }

        //cache the last position only if we still see the target (if the targets collection is not empty)
        if (aiData.currentTarget != null && aiData.targets != null && aiData.targets.Contains(aiData.currentTarget))
            targetPositionCached = aiData.currentTarget.position;

        //First check if we have reached the target
        if (Vector2.Distance(transform.position, targetPositionCached) < targetRechedThreshold)
        {
            reachedLastTarget = true; // Mark the target as reached
            aiData.currentTarget = null; // Clear the target
            return (danger, interest); // No need to continue seeking
        }

        //If we havent yet reached the target do the main logic of finding the interest directions
        Vector2 directionToTarget = (targetPositionCached - (Vector2)transform.position);
        for (int i = 0; i < interest.Length; i++) 
        {
            // Calculate the dot product between the normalized direction to the target and each of the eight cardinal and diagonal directions
            float result = Vector2.Dot(directionToTarget.normalized, Directions.eightDirections[i]);

            // Only consider directions that are within 90 degrees to the target direction
            if (result > 0)
            {
                float valueToPutIn = result;
                // Update the interest array with the highest value found
                if (valueToPutIn > interest[i])
                {
                    interest[i] = valueToPutIn;
                }

            }
        }
        interestsTemp = interest; // Store the current interest values for gizmo visualization
        return (danger, interest);  // Return the updated danger and interest arrays
    }

    /// <summary>
    /// Draws gizmos in the editor to visualize the target and the interest directions.
    /// </summary>
    private void OnDrawGizmos()
    {

        if (showGizmo == false)
            return;
        Gizmos.DrawSphere(targetPositionCached, 0.2f);

        if (Application.isPlaying && interestsTemp != null)
        {
            if (interestsTemp != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < interestsTemp.Length; i++)
                {
                    Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestsTemp[i]);
                }
                if (reachedLastTarget == false)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(targetPositionCached, 0.1f);
                }
            }
        }
    }
}
