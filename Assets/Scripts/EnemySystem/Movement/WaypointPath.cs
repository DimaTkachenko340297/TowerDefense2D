using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [SerializeField] private Transform[] _waypoints;

    private float[] _segmentDistances;

    public int PointCount => _waypoints != null ? _waypoints.Length : 0;

    private void Awake()
    {
        if (_waypoints == null || _waypoints.Length < 2)
        {
            Debug.LogError("WaypointPath::Awake() Waypoints array is empty or has insufficient points!");
            return;
        }

        CacheSegmentDistances();
    }

    private void CacheSegmentDistances()
    {
        _segmentDistances = new float[_waypoints.Length - 1];

        for (int i = 0; i < _segmentDistances.Length; i++)
        {
            if (_waypoints[i] == null || _waypoints[i + 1] == null)
            {
                Debug.LogError($"WaypointPath::CacheSegmentDistances() Waypoint transform at index {i} or {i + 1} is null!");
                _segmentDistances[i] = 1f;
                continue;
            }

            _segmentDistances[i] = Vector3.Distance(_waypoints[i].position, _waypoints[i + 1].position);
        }
    }

    public Vector3 GetPoint(int index)
    {
        if (_waypoints == null || index < 0 || index >= _waypoints.Length)
        {
            Debug.LogError($"WaypointPath::GetPoint() Invalid waypoint index: {index}");
            return Vector3.zero;
        }

        return _waypoints[index].position;
    }

    public float GetSegmentDistance(int startIndex)
    {
        if (_segmentDistances == null || startIndex < 0 || startIndex >= _segmentDistances.Length)
        {
            Debug.LogError($"WaypointPath::GetSegmentDistance() Invalid segment index: {startIndex}");
            return 1f;
        }

        return _segmentDistances[startIndex];
    }
}