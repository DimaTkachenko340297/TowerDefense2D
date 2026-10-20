using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Tower))]
public class TowerTargeting : MonoBehaviour
{
    public readonly struct ProgressInterval
    {
        public readonly float Min;
        public readonly float Max;

        public ProgressInterval(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(float progress) 
        {
            return progress >= Min && progress <= Max;
        }
    }

    // (Tower thisTower)
    public event Action<Tower> OnBecameActive;
    // (Tower thisTower)
    public event Action<Tower> OnBecameInactive;

    [SerializeField] private float _attackRange = 4f;
    
    private readonly List<int> _coveredSegmentIndices = new List<int>();
    private readonly List<ProgressInterval> _coveredIntervals = new List<ProgressInterval>();
    private IEnemyRouteTracker _enemyRouteTracker;

    public float AttackRange => _attackRange;

    [Inject]
    private void Construct(IEnemyRouteTracker enemyRouteTracker)
    {
        _enemyRouteTracker = enemyRouteTracker;
    }

    private void Start()
    {
        BakeTargetingData();
    }

    public void BakeTargetingData()
    {
        _coveredSegmentIndices.Clear();
        _coveredIntervals.Clear();
        
        if (_enemyRouteTracker == null)
        {
            Debug.LogError("TowerTargeting::BakeTargetingData() IEnemyRouteTracker was not injected!");
            return;
        }

        WaypointPath path = _enemyRouteTracker.Path;
        if (path == null)
        {
            Debug.LogError("TowerTargeting::BakeTargetingData() WaypointPath reference is missing in EnemyMovementManager!");
            return;
        }

        if (path.PointCount < 2)
        {
            Debug.LogError("TowerTargeting::BakeTargetingData() Waypoint path has insufficient points!");
            return;
        }

        for (int i = path.PointCount - 2; i >= 0; i--)
        {
            Vector3 p1 = path.GetPoint(i);
            Vector3 p2 = path.GetPoint(i + 1);

            if (TryGetSegmentCircleIntersection(p1, p2, transform.position, _attackRange, out float tMin, out float tMax))
            {
                _coveredSegmentIndices.Add(i);
                _coveredIntervals.Add(new ProgressInterval(i + tMin, i + tMax));
            }
        }
    }

    private void OnEnable()
    {
        if (_enemyRouteTracker != null)
        {
            _enemyRouteTracker.OnSegmentActivated += HandleSegmentActivated;
            _enemyRouteTracker.OnSegmentDeactivated += HandleSegmentDeactivated;
        }
    }

    private void OnDisable()
    {
        if (_enemyRouteTracker != null)
        {
            _enemyRouteTracker.OnSegmentActivated -= HandleSegmentActivated;
            _enemyRouteTracker.OnSegmentDeactivated -= HandleSegmentDeactivated;
        }
    }

    private void HandleSegmentActivated(int segmentIndex)
    {
        if (_coveredSegmentIndices.Contains(segmentIndex))
        {
            if (TryGetComponent<Tower>(out var tower))
            {
                OnBecameActive?.Invoke(tower);
            }
            else
            {
                Debug.LogError("TowerTargeting::HandleSegmentActivated() Tower component missing on this GameObject!");
            }
        }
    }

    private void HandleSegmentDeactivated(int segmentIndex)
    {
        if (_coveredSegmentIndices.Contains(segmentIndex))
        {
            if (TryGetComponent<Tower>(out var tower))
            {
                OnBecameInactive?.Invoke(tower);
            }
            else
            {
                Debug.LogError("TowerTargeting::HandleSegmentDeactivated() Tower component missing on this GameObject!");
            }
        }
    }

    public Enemy GetFirstTarget()
    {
        if (_enemyRouteTracker == null)
        {
            Debug.LogError("TowerTargeting::GetFirstTarget() IEnemyRouteTracker was not injected!");
            return null;
        }

        for (int i = 0; i < _coveredSegmentIndices.Count; i++)
        {
            int segmentIndex = _coveredSegmentIndices[i];
            IReadOnlyList<Enemy> segmentEnemies = _enemyRouteTracker.GetEnemiesOnSegment(segmentIndex);

            if (segmentEnemies == null || segmentEnemies.Count == 0)
            {
                continue;
            }

            for (int j = 0; j < segmentEnemies.Count; j++)
            {
                Enemy candidate = segmentEnemies[j];
                if (candidate == null) continue;

                float progress = candidate.GetTotalProgress();

                if (_coveredIntervals[i].Contains(progress))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private bool TryGetSegmentCircleIntersection(
        Vector3 pointA, 
        Vector3 pointB, 
        Vector3 circleCenter, 
        float radius, 
        out float tMin, 
        out float tMax)
    {
        tMin = 0f;
        tMax = 0f;

        Vector3 d = pointB - pointA;
        Vector3 f = pointA - circleCenter;

        float a = Vector3.Dot(d, d);
        float b = 2f * Vector3.Dot(f, d);
        float c = Vector3.Dot(f, f) - (radius * radius);

        // A zero-length route segment is either fully inside the circle or fully outside it.
        if (a <= Mathf.Epsilon)
        {
            if (c <= 0f)
            {
                tMin = 0f;
                tMax = 1f;
                return true;
            }

            return false;
        }

        float discriminant = (b * b) - (4f * a * c);

        if (discriminant < 0f)
        {
            return false;
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);

        float t1 = (-b - sqrtDiscriminant) / (2f * a);
        float t2 = (-b + sqrtDiscriminant) / (2f * a);

        if (t2 < 0f || t1 > 1f)
        {
            return false;
        }

        tMin = Mathf.Clamp01(t1);
        tMax = Mathf.Clamp01(t2);

        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
