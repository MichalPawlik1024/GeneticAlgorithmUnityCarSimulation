# Symulacja ewolucyjna aut na torze

Projekt Unity — auta jeżdżą przez ustalony czas po torze zdobywając punkty za przebyty dystans. Parametry jazdy ewoluują między rundami za pomocą algorytmu genetycznego.

## Architektura

```
Simulation (nadrzędny MonoBehaviour)
├── Track
│   └── TrackObject[]  (Wall / Barrier / Checkpoint / Obstacle)
├── CarBehaviour[]
│   └── Sensor          ← klasa asocjacyjna Car ↔ Track
└── GeneticAlgorithm
```

## Pliki

| Plik | Rola |
|---|---|
| `Simulation.cs` | Główny manager — spawn/destroy aut, timer rundy, wywołuje `GA.run()` po każdej rundzie |
| `Track.cs` | Kontener — zbiera `TrackObject`y ze sceny, udostępnia je sensorom i symulacji |
| `TrackObject.cs` | Abstrakcyjna baza dla obiektów toru — każdy podtyp implementuje `IsFatal()` i `GetDistanceTo()` |
| `Sensor.cs` | Klasa asocjacyjna: siedzi na aucie, strzela raycastami w `TrackObject`y, wystawia `Distances[]` |
| `CarBehaviour.cs` | Czyta `Distances[]` z sensora, porównuje z progami `DecisionSet`, wywołuje ruch; raportuje crash do `Simulation` |
| `SimulationStartPoint.cs` | Wejście scenowe, setup przed startem |
| `GeneticAlgorithm.cs` | Algorytm genetyczny — selekcja, hybrydyzacja, mutacja, elitaryzm |

## Flow symulacji

```
StartSimulation()
└── StartRound()
    ├── SpawnCars()          // jedno auto per DecisionSet
    ├── [timer]
    │   ├── CarBehaviour.Act()   // co FixedUpdate: odczyt sensorów → decyzja → ruch
    │   └── OnCarCrashed()       // gdy auto uderzy w ścianę
    └── EndRound()
        ├── ScoreCars()          // score = f(CompletedDistance)
        ├── GeneticAlgorithm.run()   // evolve → mutate → elitarism
        └── StartRound() / EndSimulation()
```

## Logika decyzji (do doimplementowania)

Sensor strzela raycastami w skonfigurowanych kierunkach i wypełnia tablicę `Distances[]`. `CarBehaviour.Act()` porównuje te odległości z progami z `DecisionSet`:

| Próg | Znaczenie |
|---|---|
| `decelerateThreshold` | jeśli odległość < progu → hamuj |
| `turnThreshold` | jeśli odległość < progu → skręć |
| `accelerateThreshold` | jeśli odległość < progu → możesz przyspieszyć |

## Konfiguracja w Unity Inspector

Na obiekcie `Simulation` ustawiasz:
- `_track` — referencja do obiektu Track w scenie
- `_carPrefab` — prefab auta z komponentami `CarBehaviour`, `Sensor`, `Rigidbody`
- `_roundDuration` — czas jednej rundy (sekundy)
- `_numberOfRounds` — liczba rund
- `_thresholdMin` / `_thresholdMax` — zakres losowania pierwszego pokolenia `DecisionSet`
- parametry GA: rozmiar populacji, liczba losowań w selekcji, szansa hybrydyzacji/mutacji

Obiekty toru (ściany, checkpointy itp.) powinny być dziećmi obiektu `Track` — `Track.ScanChildren()` zbierze je automatycznie przy starcie.
