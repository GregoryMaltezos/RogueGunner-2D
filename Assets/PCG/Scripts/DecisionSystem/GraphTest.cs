using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A test class for implementing and visualizing the Dijkstra pathfinding algorithm on a grid-based graph.
/// </summary>
public class GraphTest : MonoBehaviour
{
    Graph graph;

    bool graphReady = false;

    Dictionary<Vector2Int, int> dijkstraResult; // Stores Dijkstra algorithm results: node position -> distance
    int highestValue; // Maximum distance value from the algorithm's results

    /// <summary>
    /// Runs the Dijkstra algorithm and prepares the results for visualization.
    /// </summary>
    /// <param name="playerPosition">The starting position for the Dijkstra algorithm, typically the player's location.</param>
    /// <param name="floorPositions">The collection of valid floor positions that define the graph.</param>
    public void RunDijkstraAlgorithm(Vector2Int playerPosition,IEnumerable<Vector2Int> floorPositions)
    {
        graphReady = false;
        graph = new Graph(floorPositions);
        dijkstraResult = DijkstraAlgorithm.Dijkstra(graph, playerPosition);
        highestValue = dijkstraResult.Values.Max();
        graphReady = true;
    }

    /// <summary>
    /// Visualizes the Dijkstra results when the object is selected in the Unity editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (graphReady && dijkstraResult != null)
        {
            foreach (var item in dijkstraResult)
            {
                Color color = Color.Lerp(Color.green, Color.red, (float)item.Value / highestValue);
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawCube(item.Key + new Vector2(0.5f, 0.5f), Vector3.one);
            }
        }
    }
}
