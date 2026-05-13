using UnityEngine;

/// <summary>
/// Scene entry point. Place this on a root GameObject in the scene.
/// Simulation drives itself after Start(); this class exists for Inspector
/// visibility and any pre-simulation scene setup.
/// </summary>
public class SimulationStartPoint : MonoBehaviour
{
    [SerializeField] private Simulation _simulation;

    private void Start()
    {
        // Simulation.Start() calls StartSimulation() automatically.
        // Add any scene-level setup here (e.g. camera positioning) before the round begins.
    }
}
