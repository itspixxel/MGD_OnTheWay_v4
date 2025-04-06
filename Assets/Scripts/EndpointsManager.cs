using UnityEngine;
using System.Collections.Generic;
using OnTheWay.AI;
using System.Collections;

public class EndpointsManager : MonoBehaviour
{
    [Header("Start Point Settings")]
    [SerializeField] private GameObject housePrefab;
    [SerializeField] private Vector3Int startPosition;
    
    [Header("End Point Settings")]
    [SerializeField] private GameObject specialPrefab;
    [SerializeField] private Vector3Int endPosition;

    [Header("References")]
    [SerializeField] private PlacementManager placementManager;
    [SerializeField] private AiDirector aiDirector;

    private StructureModel startStructure;
    private StructureModel endStructure;

    private void Awake()
    {
        if (placementManager == null)
        {
            Debug.LogError("PlacementManager reference is missing in EndpointsManager!");
            enabled = false;
            return;
        }

        if (aiDirector == null)
        {
            Debug.LogError("AiDirector reference is missing in EndpointsManager!");
            enabled = false;
            return;
        }

        if (housePrefab == null || specialPrefab == null)
        {
            Debug.LogError("House or Special prefab reference is missing in EndpointsManager!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForPlacementManager());
    }

    private IEnumerator WaitForPlacementManager()
    {
        // Wait for the next frame to ensure PlacementManager's Start has been called
        yield return null;
        
        // Wait a short moment to ensure the grid is initialized
        yield return new WaitForSeconds(0.1f);
        
        SpawnEndpoints();
    }

    private void SpawnEndpoints()
    {
        if (placementManager.CheckIfPositionInBound(startPosition) && 
            placementManager.CheckIfPositionIsFree(startPosition))
        {
            // Create the structure without requiring a road
            var structure = CreateStructure(startPosition, housePrefab, CellType.Structure);
            if (structure != null)
            {
                startStructure = structure;
            }
        }

        if (placementManager.CheckIfPositionInBound(endPosition) && 
            placementManager.CheckIfPositionIsFree(endPosition))
        {
            // Create the structure without requiring a road
            var structure = CreateStructure(endPosition, specialPrefab, CellType.SpecialStructure);
            if (structure != null)
            {
                endStructure = structure;
            }
        }
    }

    private StructureModel CreateStructure(Vector3Int position, GameObject prefab, CellType type)
    {
        // Use the proper placement system but don't require a road initially
        placementManager.PlaceObjectOnTheMap(position, prefab, type, 1, 1, false);
        return placementManager.GetStructureAt(position);
    }

    public void SimulateCarMovement()
    {
        if (startStructure != null && endStructure != null && HasValidPath())
        {
            aiDirector.SpawnCarBetweenStructures(startStructure, endStructure);
        }
    }

    public bool HasValidPath()
    {
        if (startStructure == null || endStructure == null)
            return false;

        var startRoadPosition = ((INeedingRoad)startStructure).RoadPosition;
        var endRoadPosition = ((INeedingRoad)endStructure).RoadPosition;

        var path = placementManager.GetPathBetween(startRoadPosition, endRoadPosition, true);
        return path != null && path.Count > 2;
    }
} 