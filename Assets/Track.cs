using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Container for all TrackObjects in the scene.
/// Acts as the authoritative source of track geometry for sensors and the simulation.
/// Attach to a parent GameObject that groups all track elements.
/// </summary>
public class Track : MonoBehaviour
{
    // Assign in Inspector or populate via ScanChildren().
    [SerializeField] private List<TrackObject> _trackObjects = new List<TrackObject>();

    public IReadOnlyList<TrackObject> TrackObjects => _trackObjects;

    /// <summary>
    /// Returns only the fatal obstacles (walls, barriers) — used by collision checks.
    /// </summary>
    public IEnumerable<TrackObject> FatalObjects =>
        _trackObjects.Where(o => o.IsFatal());

    /// <summary>
    /// Returns only checkpoints — used for lap/scoring logic.
    /// </summary>
    public IEnumerable<TrackObject> Checkpoints =>
        _trackObjects.Where(o => o.Type == TrackObject.TrackObjectType.Checkpoint);

    /// <summary>
    /// Collects all TrackObject components from child GameObjects.
    /// Call this from Simulation.Awake() or use the Inspector list instead.
    /// </summary>
    public void ScanChildren()
    {
        _trackObjects = new List<TrackObject>(GetComponentsInChildren<TrackObject>());
    }

    private void Awake()
    {
        if (_trackObjects.Count == 0)
            ScanChildren();
    }
}
