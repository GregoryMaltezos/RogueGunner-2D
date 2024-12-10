using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Implements the Dijkstra algorithm for finding shortest paths in a graph represented as a grid.
/// </summary>
public class DijkstraAlgorithm
{

    /// <summary>
    /// Computes the shortest path distances from a starting position to all reachable nodes in the graph.
    /// </summary>
    /// <param name="graph">The graph representing the grid and its connections.</param>
    /// <param name="startPosition">The starting position for the Dijkstra algorithm.</param>
    /// <returns>
    /// A dictionary where the keys are grid positions and the values are the shortest distance from the start position.
    /// </returns>
    public static Dictionary<Vector2Int, int> Dijkstra(Graph graph, Vector2Int startposition)
    {
        Queue<Vector2Int> unfinishedVertices = new Queue<Vector2Int>();// Queue to store vertices that are yet to be processed

        Dictionary<Vector2Int, int> distanceDictionary = new Dictionary<Vector2Int, int>();  // Dictionary to store the shortest distances to each vertex
        Dictionary<Vector2Int, Vector2Int> parentDictionary = new Dictionary<Vector2Int, Vector2Int>(); // Dictionary to store the parent vertex of each node (used for reconstructing paths if needed)

        distanceDictionary[startposition] = 0;  // Initialize the starting position with a distance of 0
        parentDictionary[startposition] = startposition;// Set the starting position as its own parent

        foreach (Vector2Int vertex in graph.GetNeighbours4Directions(startposition))  // Add the neighbors of the start position to the queue
        {
            unfinishedVertices.Enqueue(vertex);
            parentDictionary[vertex] = startposition;
        }

        while (unfinishedVertices.Count > 0)   // Process the queue until all reachable vertices are visited
        {
            Vector2Int vertex = unfinishedVertices.Dequeue();
            int newDistance = distanceDictionary[parentDictionary[vertex]]+1; // Calculate the new distance based on the parent's distance
            if (distanceDictionary.ContainsKey(vertex) && distanceDictionary[vertex] <= newDistance) // Skip vertices that already have a shorter or equal distance recorded
                distanceDictionary[vertex] = newDistance;// Record the new shortest distance for the vertex

            foreach (Vector2Int neighbour in graph.GetNeighbours4Directions(vertex)) // Add unvisited neighbors to the queue and set their parent
            {
                if (distanceDictionary.ContainsKey(neighbour))  
                    continue;
                unfinishedVertices.Enqueue(neighbour);
                parentDictionary[neighbour] = vertex;
            }
        }

        return distanceDictionary; // Return the dictionary containing shortest distances
    }
}
