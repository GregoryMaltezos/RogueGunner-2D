using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidanceBehaviour : SteeringBehaviour
{
    [SerializeField]
    private float radius = 2f, agentColliderSize = 0.6f;

    [SerializeField]
    private bool showGizmo = true;

    //gizmo parameters
    float[] dangersResultTemp = null;

    /// <summary>
    /// Calculates steering behaviors based on obstacles' proximity and direction.
    /// </summary>
    /// <param name="danger">Array of danger values indicating the level of danger in each direction.</param>
    /// <param name="interest">Array of interest values (unused in this method but passed from the base class).</param>
    /// <param name="aiData">AI data that contains information about obstacles and other AI-related data.</param>
    /// <returns>Updated danger and interest arrays.</returns>
    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        foreach (Collider2D obstacleCollider in aiData.obstacles)
        {
            // Calculate direction to the closest point on the obstacle
            Vector2 directionToObstacle
                = obstacleCollider.ClosestPoint(transform.position) - (Vector2)transform.position;
            float distanceToObstacle = directionToObstacle.magnitude;

            //calculate weight based on the distance Enemy<--->Obstacle
            float weight
                = distanceToObstacle <= agentColliderSize
                ? 1
                : (radius - distanceToObstacle) / radius; // Weight increases as the obstacle gets closer

            Vector2 directionToObstacleNormalized = directionToObstacle.normalized;// Normalize the direction vector

            // Loop through the eight directions to calculate the danger in each direction
            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float result = Vector2.Dot(directionToObstacleNormalized, Directions.eightDirections[i]);
                // Multiply the result by the weight to get the final danger value
                float valueToPutIn = result * weight;

                //override value only if it is higher than the current one stored in the danger array
                if (valueToPutIn > danger[i])
                {
                    danger[i] = valueToPutIn; // Update the danger value for that direction
                }
            }
        } 
        dangersResultTemp = danger; // Store the updated danger values for visual debugging
        return (danger, interest);  // Return the updated danger and interest arrays
    }

    /// <summary>
    /// Draws gizmos in the editor to visualize the obstacle avoidance radius and danger values.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showGizmo == false)
            return;

        if (Application.isPlaying && dangersResultTemp != null)
        {
            if (dangersResultTemp != null)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < dangersResultTemp.Length; i++)
                {
                    Gizmos.DrawRay(
                        transform.position,
                        Directions.eightDirections[i] * dangersResultTemp[i]
                        );
                }
            }
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}

/// <summary>
/// List of eight possible directions (cardinal and diagonal directions)
/// </summary>
public static class Directions
{
    public static List<Vector2> eightDirections = new List<Vector2>{
            new Vector2(0,1).normalized,
            new Vector2(1,1).normalized,
            new Vector2(1,0).normalized,
            new Vector2(1,-1).normalized,
            new Vector2(0,-1).normalized,
            new Vector2(-1,-1).normalized,
            new Vector2(-1,0).normalized,
            new Vector2(-1,1).normalized
        };
}
