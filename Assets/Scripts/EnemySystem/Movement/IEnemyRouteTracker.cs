using System;
using System.Collections.Generic;

public interface IEnemyRouteTracker
{
    WaypointPath Path { get; }
    event Action<int> OnSegmentActivated;
    event Action<int> OnSegmentDeactivated;
    IReadOnlyList<Enemy> GetEnemiesOnSegment(int segmentIndex);
}
