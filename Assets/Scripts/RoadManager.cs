using SVS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnTheWay.AI;

public class RoadManager : MonoBehaviour
{
    public PlacementManager placementManager;
    public ObstacleManager obstacleManager;

    public List<Vector3Int> temporaryPlacementPositions = new List<Vector3Int>();
    public List<Vector3Int> roadPositionsToRecheck = new List<Vector3Int>();

    private Vector3Int startPosition;
    private bool placementMode = false;

    public RoadFixer roadFixer;

    private void Start()
    {
        roadFixer = GetComponent<RoadFixer>();
    }

    public void PlaceRoad(Vector3Int position)
    {
        if (placementManager.CheckIfPositionInBound(position) == false)
            return;
        if (placementManager.CheckIfPositionIsFree(position) == false)
        {
            // Check if the position has a structure that needs a road
            var structure = placementManager.GetStructureAt(position);
            if (structure != null && structure is INeedingRoad)
            {
                // Don't allow placing roads on structures that need roads
                return;
            }
        }
        if (obstacleManager != null && obstacleManager.IsObstaclePosition(position))
        {
            // Don't allow placing roads on obstacle positions
            return;
        }
        if (placementMode == false)
        {
            temporaryPlacementPositions.Clear();
            roadPositionsToRecheck.Clear();

            placementMode = true;
            startPosition = position;

            temporaryPlacementPositions.Add(position);
            placementManager.PlaceTemporaryStructure(position, roadFixer.deadEnd, CellType.Road);
        }
        else
        {
            // Get the last placed road position
            Vector3Int lastPosition = startPosition;
            if (temporaryPlacementPositions.Count > 0)
            {
                lastPosition = temporaryPlacementPositions[temporaryPlacementPositions.Count - 1];
            }

            // Calculate the path between last position and new position
            var path = GetDirectPath(lastPosition, position);
            
            // Only add new positions that aren't already in the list
            foreach (var pos in path)
            {
                if (!temporaryPlacementPositions.Contains(pos))
                {
                    temporaryPlacementPositions.Add(pos);
                    if (placementManager.CheckIfPositionIsFree(pos))
                    {
                        placementManager.PlaceTemporaryStructure(pos, roadFixer.deadEnd, CellType.Road);
                    }
                    else
                    {
                        // Check if the position has a structure that needs a road
                        var structure = placementManager.GetStructureAt(pos);
                        if (structure != null && structure is INeedingRoad)
                        {
                            // Don't allow placing roads on structures that need roads
                            continue;
                        }
                        if (obstacleManager != null && obstacleManager.IsObstaclePosition(pos))
                        {
                            // Don't allow placing roads on obstacle positions
                            continue;
                        }
                        roadPositionsToRecheck.Add(pos);
                    }
                }
            }
        }

        FixRoadPrefabs();
    }

    private List<Vector3Int> GetDirectPath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        
        // First move horizontally
        int xStep = Math.Sign(end.x - start.x);
        for (int x = start.x + xStep; x != end.x; x += xStep)
        {
            var pos = new Vector3Int(x, 0, start.z);
            // Check if this position has a structure that needs a road
            var structure = placementManager.GetStructureAt(pos);
            if (structure != null && structure is INeedingRoad)
            {
                // Stop the path at this position
                return path;
            }
            path.Add(pos);
        }
        
        // Then move vertically
        int zStep = Math.Sign(end.z - start.z);
        for (int z = start.z + zStep; z != end.z; z += zStep)
        {
            var pos = new Vector3Int(end.x, 0, z);
            // Check if this position has a structure that needs a road
            var structure = placementManager.GetStructureAt(pos);
            if (structure != null && structure is INeedingRoad)
            {
                // Stop the path at this position
                return path;
            }
            path.Add(pos);
        }
        
        // Add the end position if it's not a structure that needs a road
        var endStructure = placementManager.GetStructureAt(end);
        if (endStructure == null || !(endStructure is INeedingRoad))
        {
            path.Add(end);
        }
        
        return path;
    }

    private void FixRoadPrefabs()
    {
        foreach (var temporaryPosition in temporaryPlacementPositions)
        {
            roadFixer.FixRoadAtPosition(placementManager, temporaryPosition);
            var neighbours = placementManager.GetNeighboursOfTypeFor(temporaryPosition, CellType.Road);
            foreach (var roadposition in neighbours)
            {
                if (roadPositionsToRecheck.Contains(roadposition)==false)
                {
                    roadPositionsToRecheck.Add(roadposition);
                }
            }
        }
        foreach (var positionToFix in roadPositionsToRecheck)
        {
            roadFixer.FixRoadAtPosition(placementManager, positionToFix);
        }
    }

    public void FinishPlacingRoad()
    {
        placementMode = false;
        placementManager.AddtemporaryStructuresToStructureDictionary();
        if (temporaryPlacementPositions.Count > 0)
        {
            AudioPlayer.instance.PlayPlacementSound();
        }
        temporaryPlacementPositions.Clear();
        startPosition = Vector3Int.zero;
        UpdateSimulateButtonState();
    }

    public void ClearAllRoads()
    {
        placementManager.ClearAllRoads();
        temporaryPlacementPositions.Clear();
        roadPositionsToRecheck.Clear();
        placementMode = true;
        startPosition = Vector3Int.zero;
        UpdateSimulateButtonState();
    }

    private void UpdateSimulateButtonState()
    {
        var aiDirector = FindObjectOfType<AiDirector>();
        if (aiDirector != null)
        {
            aiDirector.UpdateButtonState();
        }
    }
}
