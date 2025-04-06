using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private ObstaclePrefabWeighted[] obstaclePrefabs;
    [SerializeField] private List<Vector3Int> obstaclePositions = new List<Vector3Int>();

    [Header("References")]
    [SerializeField] private PlacementManager placementManager;

    private float[] obstacleWeights;

    private void Awake()
    {
        if (placementManager == null)
        {
            Debug.LogError("PlacementManager reference is missing in ObstacleManager!");
            enabled = false;
            return;
        }

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogError("No obstacle prefabs assigned in ObstacleManager!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        obstacleWeights = obstaclePrefabs.Select(prefabStats => prefabStats.weight).ToArray();
        StartCoroutine(WaitForPlacementManager());
    }

    private System.Collections.IEnumerator WaitForPlacementManager()
    {
        // Wait for the next frame to ensure PlacementManager's Start has been called
        yield return null;
        
        // Wait a short moment to ensure the grid is initialized
        yield return new WaitForSeconds(0.1f);
        
        SpawnObstacles();
    }

    private void SpawnObstacles()
    {
        foreach (var position in obstaclePositions)
        {
            if (placementManager.CheckIfPositionInBound(position) && 
                placementManager.CheckIfPositionIsFree(position))
            {
                // Create the obstacle without requiring a road
                CreateObstacle(position);
            }
        }
    }

    private void CreateObstacle(Vector3Int position)
    {
        int randomIndex = GetRandomWeightedIndex(obstacleWeights);
        // Use the proper placement system but don't require a road
        placementManager.PlaceObjectOnTheMap(position, obstaclePrefabs[randomIndex].prefab, CellType.Structure, 1, 1, false);
    }

    private int GetRandomWeightedIndex(float[] weights)
    {
        float totalWeight = weights.Sum();
        float randomValue = Random.Range(0, totalWeight);
        float currentWeight = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            currentWeight += weights[i];
            if (randomValue <= currentWeight)
            {
                return i;
            }
        }

        return 0; // Fallback to first index if something goes wrong
    }

    public void AddObstaclePosition(Vector3Int position)
    {
        if (!obstaclePositions.Contains(position))
        {
            obstaclePositions.Add(position);
        }
    }

    public void RemoveObstaclePosition(Vector3Int position)
    {
        obstaclePositions.Remove(position);
    }

    public void ClearObstacles()
    {
        foreach (var position in obstaclePositions)
        {
            var structure = placementManager.GetStructureAt(position);
            if (structure != null)
            {
                Destroy(structure.gameObject);
                placementManager.GetGrid()[position.x, position.z] = CellType.Empty;
            }
        }
        obstaclePositions.Clear();
    }

    public bool IsObstaclePosition(Vector3Int position)
    {
        return obstaclePositions.Contains(position);
    }
} 