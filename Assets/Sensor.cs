using UnityEngine;

/// <summary>
/// Associative class linking a CarBehaviour to the Track.
/// Casts rays in configured directions and reports distances to the nearest TrackObjects.
/// Attach as a component on the same GameObject as CarBehaviour.
///
/// The measured distances are compared against the car's DecisionSet thresholds:
///   distance &lt; decelerateThreshold  → car should brake
///   distance &lt; turnThreshold        → car should steer
///   distance &lt; accelerateThreshold  → car may accelerate
/// </summary>
public class Sensor : MonoBehaviour
{
    [Header("Ray configuration")]
    [SerializeField] private float _maxRange = 20f;
    [SerializeField] private LayerMask _detectionLayers;

    // One entry per ray direction; indices are stable across frames.
    [SerializeField] private Vector3[] _rayDirections;

    // Results written each FixedUpdate; read by CarBehaviour.
    private float[] _distances;

    public float MaxRange => _maxRange;

    /// <summary>Current distance reading per ray direction. NaN = no hit within range.</summary>
    public float[] Distances => _distances;

    private CarBehaviour _car;

    private void Awake()
    {
        _car = GetComponent<CarBehaviour>();
        _distances = new float[_rayDirections?.Length ?? 0];
    }

    private void FixedUpdate()
    {
        MeasureDistances();
    }

    /// <summary>
    /// Fires all configured rays and populates <see cref="Distances"/>.
    /// Called automatically in FixedUpdate; can also be called manually for testing.
    /// Implementation: iterate _rayDirections, Physics.Raycast, write hit.distance or _maxRange.
    /// </summary>
    public void MeasureDistances()
    {
        for (Vector3 direction in )
        // TODO: implement raycasting logic
    }

    /// <summary>
    /// Returns the smallest distance across all rays — convenience for deceleration checks.
    /// </summary>
    public float MinDistance()
    {
        return Mathf.Min(_distances);

    }

}
