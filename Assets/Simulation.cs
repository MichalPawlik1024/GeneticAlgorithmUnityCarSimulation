using GeneticAlgorithm;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

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
    private DecisionSet _thresholdMin = new DecisionSet();
    private DecisionSet _thresholdMax = new DecisionSet();

    // ── State ────────────────────────────────────────────────────────────────

    private GeneticAlgorithm.GeneticAlgorithm _geneticAlgorithm;
    private List<Car> _activeCars = new List<Car>();

    private int _currentRound = 0;
    private float _roundTimeRemaining;
    private bool _roundRunning = false;
    private bool _isDemoRound = false;

    private string _resultsFilePath;
    private DecisionSet _bestOverallDecisionSet;

    public int CurrentRound => _currentRound;
    public float RoundTimeRemaining => _roundTimeRemaining;

    private float sensorTimer = 0f;

    public LayerMask WallLayerMask;
    public float MaxSensorDistance = 20f;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        // TODO: validate references
    }

    private void Start()
    {
        StartSimulation();
    }

    private void FixedUpdate()
    {
        if (sensorTimer > 0f)
        {
            sensorTimer -= Time.fixedDeltaTime;
        }
        else
        {
            sensorTimer = 0.1f;
            updateSensors();
        }

        foreach (Car car in _activeCars)
        {
            car.Simulate();
        }
    }

    private void updateSensors()
    {
        int totalSensors = _activeCars.Count * 7;

        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(totalSensors, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(totalSensors, Allocator.TempJob);

        int index = 0;
        foreach (Car car in _activeCars)
        {
            if (car.IsDeactivated) // Pomiń zniszczone auta, żeby oszczędzić CPU!
            {
                index += 7;
                continue;
            }

            for (int s = 0; s < 7; s++)
            {
                Transform sensorTransform = car.Sensors[s].transform;
                commands[index] = new RaycastCommand(
                    sensorTransform.position,
                    sensorTransform.forward,
                    new QueryParameters(WallLayerMask.value, false, QueryTriggerInteraction.Ignore, false),
                    MaxSensorDistance
                );
                index++;
            }
        }

        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 64, default);
        handle.Complete();

        index = 0;
        foreach (Car car in _activeCars)
        {
            if (car.IsDeactivated)
            {
                index += 7;
                continue;
            }

            for (int s = 0; s < 7; s++)
            {
                RaycastHit hit = results[index];
                if (hit.collider != null)
                    car.SensorValues[s] = hit.distance / MaxSensorDistance;
                else
                    car.SensorValues[s] = 1f;

                index++;
            }
        }

        // 5. Koniecznie zwolnij pamięć!
        commands.Dispose();
        results.Dispose();
    }

    private void Update()
    {
        if (_isDemoRound && Input.GetKeyDown(KeyCode.Q))
        {
            StopDemoRound();
            return;
        }

        if (!_isDemoRound && Input.GetKeyDown(KeyCode.B))
        {
            if (_bestOverallDecisionSet != null)
                StartDemoRound();
            return;
        }

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

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _resultsFilePath = Path.Combine(Application.persistentDataPath, $"ga_results_{timestamp}.csv");
        File.WriteAllText(_resultsFilePath,
            "round,best_score,avg_score,worst_score," +
            "best_turnThreshold,best_accelerateThreshold,best_decelerateThreshold," +
            "best_steerValue,best_accelerateValue,best_decelerateValue\n");
        Debug.Log($"[Simulation] Saving results to: {_resultsFilePath}");

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
        if (_isDemoRound)
        {
            EndDemoRound();
            return;
        }

        _roundRunning = false;
        ScoreCars();
        var elite = _geneticAlgorithm.decisionSets.OrderByDescending(d => d.score).First();
        Debug.Log($"Round {_currentRound} | Elite score: {elite.score:F2} | Best this round: {_geneticAlgorithm.decisionSets.Max(d => d.score):F2}");
        AppendRoundResults();
        AppendRoundResults();
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
        Debug.Log($"[Simulation] Finished. Results saved to: {_resultsFilePath}");
        GenerateCharts();
        StartDemoRound();
    }

    private void GenerateCharts()
    {
        string scriptPath = Path.Combine(Application.dataPath, "MakeCharts.py");
        string outputBase = _resultsFilePath.Replace(".csv", "");

        if (!File.Exists(scriptPath))
        {
            Debug.LogWarning($"[Simulation] MakeCharts.py not found at: {scriptPath}");
            return;
        }

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"\"{scriptPath}\" \"{_resultsFilePath}\" \"{outputBase}.png\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();
            string err = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(err))
                Debug.LogWarning($"[MakeCharts] {err}");
            else
                Debug.Log($"[MakeCharts] Charts saved to: {outputBase}Scores.png / {outputBase}Values.png");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MakeCharts] Failed to run python3: {e.Message}");
        }
    }

    private void StartDemoRound()
    {
        if (_bestOverallDecisionSet == null) return;

        Debug.Log($"[Simulation] Demo run — best score: {_bestOverallDecisionSet.score:F4}");
        _isDemoRound = true;
        _roundTimeRemaining = _roundDuration*10;
        _activeCars.Clear();
        SpawnCars(new List<DecisionSet> { _bestOverallDecisionSet });
        _roundRunning = true;
    }

    private void EndDemoRound()
    {
        DestroyAllCars();
        // Loop: restart demo immediately instead of stopping
        Debug.Log("[Simulation] Demo round restarting...");
        StartDemoRound();
    }

    private void StopDemoRound()
    {
        _roundRunning = false;
        _isDemoRound = false;
        DestroyAllCars();
        Debug.Log("[Simulation] Demo run stopped.");
    }

    private void AppendRoundResults()
    {
        var sets = _geneticAlgorithm.getDecisionSets();
        if (sets == null || sets.Count == 0) return;

        double worst = sets.Min(d => d.score);
        double avg = sets.Average(d => d.score);
        DecisionSet bestDs = sets.OrderByDescending(d => d.score).First();

        // Aktualizuj all-time best, potem użyj go do CSV
        if (_bestOverallDecisionSet == null || bestDs.score > _bestOverallDecisionSet.score)
            _bestOverallDecisionSet = bestDs;

        double best = _bestOverallDecisionSet.score; // monotonicznie rosnący

        float TODOexportValues = 0.0f;

        string line = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "{0},{1:F4},{2:F4},{3:F4},{4:F6},{5:F6},{6:F6},{7:F6},{8:F6},{9:F6}\n",
            _currentRound, best, avg, worst,
            TODOexportValues, TODOexportValues, TODOexportValues,
            TODOexportValues, TODOexportValues, TODOexportValues);

        File.AppendAllText(_resultsFilePath, line);
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
