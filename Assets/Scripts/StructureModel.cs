using OnTheWay.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureModel : MonoBehaviour, INeedingRoad
{
    float yHeight = 0;

    public Vector3Int RoadPosition { get; set; }

    public void CreateModel(GameObject model)
    {
        var structure = Instantiate(model, transform);
        yHeight = structure.transform.position.y;
    }

    public void SwapModel(GameObject model, Quaternion rotation)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        var structure = Instantiate(model, transform);
        structure.transform.localPosition = new Vector3(0, yHeight, 0);
        structure.transform.localRotation = rotation;
    }

    public Vector3 GetNearestPedestrianMarkerTo(Vector3 position)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetClosestPedestrainPosition(position);
    }

    public Marker GetPedestrianSpawnMarker(Vector3 position)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetpositioForPedestrianToSpwan(position);
    }

    public List<Marker> GetPedestrianMarkers()
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetAllPedestrianMarkers();
    }

    internal List<Marker> GetCarMarkers()
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetAllCarMarkers();
    }

    public Vector3 GetNearestCarMarkerTo(Vector3 position)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetClosestCarMarkerPosition(position);
    }

    public Marker GetCarSpawnMarker(Vector3Int nextPathPosition)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetPositioForCarToSpawn(nextPathPosition);
    }

    public Marker GetCarEndMarker(Vector3Int previousPathPosition)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetPositioForCarToEnd(previousPathPosition);
    }
}
