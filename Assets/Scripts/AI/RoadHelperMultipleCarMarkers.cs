using OnTheWay.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadHelperMultipleCarMarkers : RoadHelper
{
    [SerializeField]
    protected List<Marker> incommingMarkers, outgoingMarkers;

    public override Marker GetPositioForCarToSpawn(Vector3 nextPathPosition)
    {
        return GetClosestMarkeTo(nextPathPosition, outgoingMarkers);
    }

    public override Marker GetPositioForCarToEnd(Vector3 previousPathPosition)
    {
        return GetClosestMarkeTo(previousPathPosition, incommingMarkers);
    }
}
