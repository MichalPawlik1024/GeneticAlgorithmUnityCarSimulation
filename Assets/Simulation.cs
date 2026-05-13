using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneticAlgorithm;

/// <summary>
/// Top-level manager. Owns the Track, all CarBehaviours, and the GeneticAlgorithm.
/// One round = spawn all cars, let them drive until time runs out or they crash,
/// then score them and evolve the next generation.
/// </summary>
public class Simulation : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private GameObject _carPrefab;
    [SerializeField] private GameObject _simulationStartPoint;

    [Header("Simulation parameters")]
    [SerializeField] private float _roundDuration = 30f;   // seconds per round
    [SerializeField] private int _numberOfRounds = 10;

    [Header("Genetic algorithm parameters")]
    [SerializeField] private int _populationSize = 20;
    [SerializeField] private int _selectionDrawCount = 5;
    [SerializeField] private int _hybridizationChancePercent = 70;
    [SerializeField] private int _mutationChancePercent = 10;
    [SerializeField] private DecisionSet _thresholdMin;
    [SerializeField] private DecisionSet _thresholdMax;

    // ── State ────────────────────────────────────────────────────────────────

    private GeneticAlgorithm.GeneticAlgorithm _geneticAlgorithm;
    private List<Car> _activeCars = new List<Car>();

    private int _currentRound = 0;
    private float _roundTimeRemaining;
    private bool _roundRunning = false;

    public int CurrentRound => _currentRound;
    public float RoundTimeRemaining => _roundTimeRemaining;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        // TODO: validate references
    }

    private void Start()
    {
        StartSimulation();
    }

    private void Update()
    {
        if (!_roundRunning) return;

        _roundTimeRemaining -= Time.deltaTime;
        if (_roundTimeRemaining <= 0f)
            EndRound();
    }

    // ── Simulation flow ───────────────────────────────────────────────────────

    /// <summary>Initialises the genetic algorithm and kicks off round 1.</summary>
    public void StartSimulation()
    {
        _geneticAlgorithm = new GeneticAlgorithm.GeneticAlgorithm(
            _populationSize,
            _thresholdMin,
            _thresholdMax,
            _selectionDrawCount,
            _hybridizationChancePercent,
            _mutationChancePercent
        );

        _currentRound = 0;
        StartRound();
    }

    /// <summary>Spawns one car per DecisionSet and starts the timer.</summary>
    private void StartRound()
    {
        _currentRound++;
        _roundTimeRemaining = _roundDuration;
        _activeCars.Clear();

        SpawnCars(_geneticAlgorithm.getDecisionSets());

        _roundRunning = true;
    }

    /// <summary>
    /// Called when the round timer hits zero.
    /// Scores surviving cars, evolves the population, optionally starts next round.
    /// </summary>
    private void EndRound()
    {
        _roundRunning = false;
        ScoreCars();
        DestroyAllCars();

        _geneticAlgorithm.run();

        if (_currentRound < _numberOfRounds)
            StartRound();
        else
            EndSimulation();
    }

    /// <summary>Called when all configured rounds have completed.</summary>
    private void EndSimulation()
    {
        // TODO: display/export results
    }

    // ── Car management ────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates one car GameObject per DecisionSet at the designated spawn point.
    /// Attaches (or configures) Sensor components on each car.
    /// </summary>
    private void SpawnCars(List<DecisionSet> decisionSets)
    {
        foreach (var ds in decisionSets)
        {
            // TODO: Instantiate _carPrefab, set car.decisionSet = ds, register sensors
            Car newCar = Instantiate(_carPrefab, _simulationStartPoint.transform.position, _simulationStartPoint.transform.rotation, transform).GetComponent<Car>();
            newCar.DecisionSet = ds;
            _activeCars.Add(newCar);
        }
    }

    /// <summary>
    /// Destroys all active car GameObjects and clears the list.
    /// </summary>
    private void DestroyAllCars()
    {
        foreach (var car in _activeCars)
        {
            if (car != null)
                Destroy(car.gameObject);
        }
        _activeCars.Clear();
    }

    /// <summary>
    /// Writes the final score into each car's DecisionSet based on CompletedDistance.
    /// Called at the end of a round for all surviving cars.
    /// </summary>
    private void ScoreCars()
    {
        foreach (var car in _activeCars)
        {
            float distance = (car.transform.position - _simulationStartPoint.transform.position).magnitude;
            car.DecisionSet.score = distance;
        }
    }
}
