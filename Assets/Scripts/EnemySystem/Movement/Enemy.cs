using UnityEngine;

public class Enemy : MonoBehaviour
{
    private GameObject _originPrefab;
    public GameObject OriginPrefab
    {
        get => _originPrefab;
        set
        {
            if (_originPrefab == null && value != null)
            {
                _originPrefab = value;
            }
        }
    }

    [SerializeField, Min(0.1f)] private float _moveSpeed = 3f;

    public float MoveSpeed => _moveSpeed;
    private int _currentWaypointIndex;
    public int CurrentWaypointIndex { 
        get =>_currentWaypointIndex; 
        set 
        {
            _currentWaypointIndex = Mathf.Max(value, 0);
        } 
    }
    private float _lerpProgress;
    public float LerpProgress {
        get => _lerpProgress;
        set
        {
            _lerpProgress = Mathf.Clamp01(value);
        } 
    }

    public void ResetMovementData()
    {
        CurrentWaypointIndex = 0;
        LerpProgress = 0f;
    }
}