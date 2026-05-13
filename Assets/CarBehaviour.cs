using System.Collections.Generic;
using UnityEngine;
using GeneticAlgorithm;

/// <summary>
/// Controls a single car for one simulation round.
/// Reads sensor distances, compares them against DecisionSet thresholds,
/// and drives the car accordingly.
/// Reports crashes to the parent Simulation.
/// </summary>
public class CarBehaviour : MonoBehaviour
{
    // ── Data ─────────────────────────────────────────────────────────────────

    public DecisionSet decisionSet;

    /// <summary>Total distance driven this round. Written each FixedUpdate.</summary>
    public double CompletedDistance;

    // ── References ────────────────────────────────────────────────────────────

    private Simulation _simulation;
    private Rigidbody _rigidbody;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _crashed = false;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _simulation = GetComponentInParent<Simulation>();
    }

    private void FixedUpdate()
    {
        if (_crashed) return;

        Act();
        UpdateDistance();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (false)
        {
            HandleCrash();
        }
    }

    // ── Decision making ───────────────────────────────────────────────────────

    /// <summary>
    /// Reads current sensor distances, applies DecisionSet thresholds,
    /// and issues movement commands.
    /// Threshold semantics (from DecisionSet):
    ///   distance &lt; decelerateThreshold → Decelerate()
    ///   distance &lt; turnThreshold       → Turn()
    ///   distance &lt; accelerateThreshold → Accelerate() (when clear)
    /// </summary>
    private void Act()
    {
        // TODO: read _sensor.Distances, compare to decisionSet thresholds, call movement methods
    }

    // ── Movement primitives (stub — implement physics here) ───────────────────

    private void Accelerate()
    {
        // TODO: apply forward force / increase speed
    }

    private void Decelerate()
    {
        // TODO: apply braking force / decrease speed
    }

    /// <summary>
    /// Steers the car. Direction (+/-) determined by which side has more clearance.
    /// </summary>
    private void Turn(float direction)
    {
        // TODO: apply torque or adjust steering angle
    }

    // ── Scoring ───────────────────────────────────────────────────────────────

    /// <summary>Accumulates travelled distance from physics velocity.</summary>
    private void UpdateDistance()
    {
        // TODO: CompletedDistance += _rigidbody.velocity.magnitude * Time.fixedDeltaTime
    }

    // ── Crash handling ────────────────────────────────────────────────────────

    private void HandleCrash()
    {
        if (_crashed) return;
        _crashed = true;
        _simulation?.OnCarCrashed(this);
    }
}
