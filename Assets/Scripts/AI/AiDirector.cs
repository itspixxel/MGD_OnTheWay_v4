using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OnTheWay.AI
{
    public class AiDirector : MonoBehaviour
    {
        public PlacementManager placementManager;
        public GameObject[] pedestrianPrefabs;
        public GameObject carPrefab;
        public Button spawnCarButton;

        private bool isCarActive = false;
        public bool IsCarActive => isCarActive;

        AdjacencyGraph pedestrianGraph = new AdjacencyGraph();
        AdjacencyGraph carGraph = new AdjacencyGraph();

        List<Vector3> carPath = new List<Vector3>();

        private void Start()
        {
            if (spawnCarButton != null)
            {
                spawnCarButton.onClick.AddListener(SpawnACar);
                UpdateButtonState();
            }
        }

        public void UpdateButtonState()
        {
            if (spawnCarButton != null)
            {
                var endpointsManager = FindFirstObjectByType<EndpointsManager>();
                spawnCarButton.interactable = endpointsManager != null && endpointsManager.HasValidPath() && !isCarActive;
            }
        }

        public void SpawnAllAagents()
        {
            foreach (var house in placementManager.GetAllHouses())
            {
                TrySpawningAnAgent(house, placementManager.GetRandomSpecialStrucutre());
            }
            foreach (var specialStructure in placementManager.GetAllSpecialStructures())
            {
                TrySpawningAnAgent(specialStructure, placementManager.GetRandomHouseStructure());
            }
        }

        private void TrySpawningAnAgent(StructureModel startStructure, StructureModel endStructure)
        {
            if(startStructure != null && endStructure != null)
            {
                var startPosition = ((INeedingRoad)startStructure).RoadPosition;
                var endPosition = ((INeedingRoad)endStructure).RoadPosition;

                var startMarkerPosition = placementManager.GetStructureAt(startPosition).GetPedestrianSpawnMarker(startStructure.transform.position);
                var endMarkerPosition = placementManager.GetStructureAt(endPosition).GetNearestPedestrianMarkerTo(endStructure.transform.position);

                var agent = Instantiate(GetRandomPedestrian(), startMarkerPosition.Position, Quaternion.identity);
                var path = placementManager.GetPathBetween(startPosition, endPosition, true);
                if(path.Count > 0)
                {
                    path.Reverse();
                    List<Vector3> agentPath = GetPedestrianPath(path, startMarkerPosition.Position, endMarkerPosition);
                    var aiAgent = agent.GetComponent<AiAgent>();
                    aiAgent.Initialize(agentPath);
                }
            }
        }

        public void SpawnACar()
        {
            if (isCarActive) return;
            
            foreach (var house in placementManager.GetAllHouses())
            {
                TrySpawninACar(house, placementManager.GetRandomSpecialStrucutre());
                GameObject.Find("InputSystem").GetComponent<InputManager>().enabled = false;
            }
        }

        public void SpawnCarBetweenStructures(StructureModel startStructure, StructureModel endStructure)
        {
            if (startStructure != null && endStructure != null)
            {
                TrySpawninACar(startStructure, endStructure);
            }
        }

        private void TrySpawninACar(StructureModel startStructure, StructureModel endStructure)
        {
            if (startStructure != null && endStructure != null)
            {
                var startRoadPosition = ((INeedingRoad)startStructure).RoadPosition;
                var endRoadPosition = ((INeedingRoad)endStructure).RoadPosition;

                var path = placementManager.GetPathBetween(startRoadPosition, endRoadPosition, true);
                path.Reverse();

                if (path.Count == 0 || path.Count <= 2)
                    return;

                var startMarkerPosition = placementManager.GetStructureAt(startRoadPosition).GetCarSpawnMarker(path[1]);
                var endMarkerPosition = placementManager.GetStructureAt(endRoadPosition).GetCarEndMarker(path[path.Count-2]);

                // Check if there are obstacles near the spawn position
                var obstacleManager = FindFirstObjectByType<ObstacleManager>();
                if (obstacleManager != null)
                {
                    var spawnGridPosition = Vector3Int.RoundToInt(startMarkerPosition.Position);
                    var nearbyPositions = new List<Vector3Int>
                    {
                        spawnGridPosition,
                        spawnGridPosition + Vector3Int.right,
                        spawnGridPosition + Vector3Int.left,
                        spawnGridPosition + Vector3Int.forward,
                        spawnGridPosition + Vector3Int.back
                    };

                    foreach (var pos in nearbyPositions)
                    {
                        if (obstacleManager.IsObstaclePosition(pos))
                        {
                            Debug.Log("Cannot spawn car near obstacle at position: " + pos);
                            return;
                        }
                    }
                }

                carPath = GetCarPath(path, startMarkerPosition.Position, endMarkerPosition.Position);

                if(carPath.Count > 0)
                {
                    var car = Instantiate(carPrefab, startMarkerPosition.Position, Quaternion.identity);
                    var carAI = car.GetComponent<CarAI>();
                    carAI.SetPath(carPath);
                    carAI.OnCarReachedDestination.AddListener(OnCarReachedDestination);
                    isCarActive = true;
                    if (spawnCarButton != null)
                    {
                        spawnCarButton.interactable = false;
                    }
                }
            }
        }

        private void OnCarReachedDestination()
        {
            isCarActive = false;
            if (spawnCarButton != null)
            {
                spawnCarButton.interactable = true;
            }
        }

        private List<Vector3> GetPedestrianPath(List<Vector3Int> path, Vector3 startPosition, Vector3 endPosition)
        {
            pedestrianGraph.ClearGraph();
            CreatAPedestrianGraph(path);
            Debug.Log(pedestrianGraph);
            return AdjacencyGraph.AStarSearch(pedestrianGraph,startPosition,endPosition);
        }

        private void CreatAPedestrianGraph(List<Vector3Int> path)
        {
            Dictionary<Marker, Vector3> tempDictionary = new Dictionary<Marker, Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                var currentPosition = path[i];
                var roadStructure = placementManager.GetStructureAt(currentPosition);
                var markersList = roadStructure.GetPedestrianMarkers();
                bool limitDistance = markersList.Count == 4;
                tempDictionary.Clear();
                foreach (var marker in markersList)
                {
                    pedestrianGraph.AddVertex(marker.Position);
                    foreach (var markerNeighbourPosition in marker.GetAdjacentPositions())
                    {
                        pedestrianGraph.AddEdge(marker.Position, markerNeighbourPosition);
                    }

                    if(marker.OpenForconnections && i+1 < path.Count)
                    {
                        var nextRoadStructure = placementManager.GetStructureAt(path[i + 1]);
                        if (limitDistance)
                        {
                            tempDictionary.Add(marker, nextRoadStructure.GetNearestPedestrianMarkerTo(marker.Position));
                        }
                        else
                        {
                            pedestrianGraph.AddEdge(marker.Position, nextRoadStructure.GetNearestPedestrianMarkerTo(marker.Position));
                        }
                    }
                }
                if(limitDistance && tempDictionary.Count == 4)
                {
                    var distanceSortedMarkers = tempDictionary.OrderBy(x => Vector3.Distance(x.Key.Position, x.Value)).ToList();
                    for (int j = 0; j < 2; j++)
                    {
                        pedestrianGraph.AddEdge(distanceSortedMarkers[j].Key.Position, distanceSortedMarkers[j].Value);
                    }
                }
            }
        }

        private List<Vector3> GetCarPath(List<Vector3Int> path, Vector3 startPosition, Vector3 endPosition)
        {
            carGraph.ClearGraph();
            CreatACarGraph(path);
            Debug.Log(carGraph);
            return AdjacencyGraph.AStarSearch(carGraph, startPosition, endPosition);
        }

        private void CreatACarGraph(List<Vector3Int> path)
        {
            Dictionary<Marker, Vector3> tempDictionary = new Dictionary<Marker, Vector3>();
            for (int i = 0; i < path.Count; i++)
            {
                var currentPosition = path[i];
                var roadStructure = placementManager.GetStructureAt(currentPosition);
                var markersList = roadStructure.GetCarMarkers();
                var limitDistance = markersList.Count > 3;
                tempDictionary.Clear();

                foreach (var marker in markersList)
                {
                    carGraph.AddVertex(marker.Position);
                    foreach (var markerNeighbour in marker.adjacentMarkers)
                    {
                        carGraph.AddEdge(marker.Position, markerNeighbour.Position);
                    }
                    if(marker.OpenForconnections && i + 1 < path.Count)
                    {
                        var nextRoadPosition = placementManager.GetStructureAt(path[i + 1]);
                        if (limitDistance)
                        {
                            tempDictionary.Add(marker, nextRoadPosition.GetNearestCarMarkerTo(marker.Position));
                        }
                        else
                        {
                            carGraph.AddEdge(marker.Position, nextRoadPosition.GetNearestCarMarkerTo(marker.Position));
                        }
                    }
                }
                if (limitDistance && tempDictionary.Count > 2)
                {
                    var distanceSortedMarkers = tempDictionary.OrderBy(x => Vector3.Distance(x.Key.Position, x.Value)).ToList();
                    foreach (var item in distanceSortedMarkers)
                    {
                        Debug.Log(Vector3.Distance(item.Key.Position, item.Value));
                    }
                    for (int j = 0; j < 2; j++)
                    {
                        carGraph.AddEdge(distanceSortedMarkers[j].Key.Position, distanceSortedMarkers[j].Value);
                    }
                }
            }
        }

        private GameObject GetRandomPedestrian()
        {
            return pedestrianPrefabs[UnityEngine.Random.Range(0, pedestrianPrefabs.Length)];
        }

        private void Update()
        {
            //DrawGraph(carGraph);
            for (int i = 1; i < carPath.Count; i++)
            {
                Debug.DrawLine(carPath[i - 1] + Vector3.up, carPath[i] + Vector3.up, Color.magenta);
            }
        }

        private void DrawGraph(AdjacencyGraph graph)
        {
            foreach (var vertex in graph.GetVertices())
            {
                foreach (var vertexNeighbour in graph.GetConnectedVerticesTo(vertex))
                {
                    Debug.DrawLine(vertex.Position + Vector3.up, vertexNeighbour.Position + Vector3.up, Color.red);
                }
            }
        }
    }
}

