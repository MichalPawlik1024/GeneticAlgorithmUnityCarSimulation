using UnityEngine;

/// <summary>
/// Base class for every physical element placed on the track (walls, barriers, checkpoints, obstacles).
/// Attach subclasses to GameObjects in the scene; Track collects them.
/// </summary>
public abstract class TrackObject : MonoBehaviour
{
    public enum TrackObjectType
    {
        Wall,
        Barrier,
        Checkpoint,
        Obstacle
    }

    [SerializeField] private TrackObjectType _type;

    public TrackObjectType Type => _type;

    /// <summary>
    /// Called by Sensor when a car's ray hits this object.
    /// Returns the distance from the ray origin to the contact point.
    /// Implementation: use the provided hit info.
    /// </summary>
    public abstract float GetDistanceTo(RaycastHit hit);

    /// <summary>
    /// Returns true if a collision with this object should end the car's run (e.g. a wall).
    /// </summary>
    public abstract bool IsFatal();
}
