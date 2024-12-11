using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextSolver : MonoBehaviour
{
    [SerializeField]
    private bool showGizmos = true;

    //gozmo parameters
    float[] interestGizmo = new float[0];
    Vector2 resultDirection = Vector2.zero;
    private float rayLength = 1;


    /// <summary>
    /// initializes the interestGizmo array with the appropriate size.
    /// </summary>
    private void Start()
    {
        interestGizmo = new float[8];
    }

    /// <summary>
    /// Calculates the direction to move based on the steering behaviors and AI data.
    /// </summary>
    /// <param name="behaviours">List of steering behaviors that contribute to the calculation.</param>
    /// <param name="aiData">The AI data containing necessary context information.</param>
    /// <returns>A Vector2 representing the movement direction.</returns>
    public Vector2 GetDirectionToMove(List<SteeringBehaviour> behaviours, AIData aiData)
    {
        // Arrays to store danger and interest values for each direction
        float[] danger = new float[8];
        float[] interest = new float[8];

        // Loop through each behaviour to aggregate danger and interest values
        foreach (SteeringBehaviour behaviour in behaviours)
        {
            (danger, interest) = behaviour.GetSteering(danger, interest, aiData);
        }

        // Subtract danger values from the interest array and clamp the results between 0 and 1
        for (int i = 0; i < 8; i++)
        {
            interest[i] = Mathf.Clamp01(interest[i] - danger[i]);
        }
        // Update the interestGizmo array for visualization purposes
        interestGizmo = interest;

        // Calculate the average direction based on interest values
        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            outputDirection += Directions.eightDirections[i] * interest[i];
        }
        outputDirection.Normalize();
        // Store the result direction for gizmo drawing
        resultDirection = outputDirection;

        //return the selected movement direction
        return resultDirection;
    }

    /// <summary>
    /// Draws gizmos in the scene view to visualize the calculated direction and other debugging info.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && showGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, resultDirection * rayLength);
        }
    }
}
