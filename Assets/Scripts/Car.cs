using GeneticAlgorithm;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public GameObject SuspensionOriginFL;
    public GameObject SuspensionOriginFR;
    public Wheel WheelFL;
    public Wheel WheelFR;
    public Wheel WheelRL;
    public Wheel WheelRR;

    public Sensor[] Sensors;

    public LayerMask WallLayerMask;

    public float MaxEngineMomentum = 1f;
    public float MaxSteeringAngle = 30f;

    public bool IsDeactivated = false;

    public DecisionSet DecisionSet;

    public float UpdateHz = 10.0f;
    public float UpdateHzVariation = 2.0f;

    public float[] SensorValues = new float[7];

    private float steering = 0f;
    private float engine = 0f;
    private float timer = 0f;

    private float[] inputLayer = new float[DecisionSet.INPUT_LAYER_N];
    private float[] hiddenLayer1 = new float[DecisionSet.HIDDEN_LAYER_1_N];
    private float[] hiddenLayer2 = new float[DecisionSet.HIDDEN_LAYER_2_N];
    private float[] outputLayer = new float[DecisionSet.OUTPUT_LAYER_N];

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 1f / (UpdateHz + UnityEngine.Random.Range(-UpdateHzVariation, UpdateHzVariation));

            var rb = GetComponent<Rigidbody>();
            if (rb.isKinematic)
                return;
            Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

            steering = 0f;
            engine = 0f;

            int maxSensors = 9;
            for (int i = 0; i < Math.Min(maxSensors, Sensors.Length); i++)
            {
                //inputLayer[i] = Sensors[i].GetValue();
                inputLayer[i] = SensorValues[i];
            }

            inputLayer[9] = localVelocity.z / 50f;
            inputLayer[10] = localVelocity.x / 10f;


            // Hidden Layer 1 - ReLU
            MatrixMulVector(DecisionSet.hiddenLayer1Weights, DecisionSet.HIDDEN_LAYER_1_N, DecisionSet.INPUT_LAYER_N, inputLayer, hiddenLayer1);
            VectorAddInPlace(hiddenLayer1, DecisionSet.hiddenLayer1Biases);
            VectorReLUInPlace(hiddenLayer1);

            // Hidden Layer 2 - ReLU
            MatrixMulVector(DecisionSet.hiddenLayer2Weights, DecisionSet.HIDDEN_LAYER_2_N, DecisionSet.HIDDEN_LAYER_1_N, hiddenLayer1, hiddenLayer2);
            VectorAddInPlace(hiddenLayer2, DecisionSet.hiddenLayer2Biases);
            VectorReLUInPlace(hiddenLayer2);

            // Output Layer - Tanh
            MatrixMulVector(DecisionSet.outputLayerWeights, DecisionSet.OUTPUT_LAYER_N, DecisionSet.HIDDEN_LAYER_2_N, hiddenLayer2, outputLayer);
            VectorAddInPlace(outputLayer, DecisionSet.outputLayerBiases);
            VectorTanhInPlace(outputLayer);

            engine = outputLayer[0];
            steering = outputLayer[1];

            //float steering = Input.GetAxis("Horizontal");
            SuspensionOriginFL.transform.localRotation = Quaternion.Euler(0f, steering * MaxSteeringAngle, 0f);
            SuspensionOriginFR.transform.localRotation = Quaternion.Euler(0f, steering * MaxSteeringAngle, 0f);

            //float engine = Input.GetAxis("Vertical");
            WheelRL.EngineMomentum = engine * MaxEngineMomentum;
            WheelRR.EngineMomentum = engine * MaxEngineMomentum;
        }
    }

    // Update is called once per frame
    public void Simulate()
    {
        

        WheelFL.Simulate();
        WheelFR.Simulate();
        WheelRL.Simulate();
        WheelRR.Simulate();
    }

    private void MatrixMulVector(double[] matrix, int M, int N, float[] inputVector, float[] outputVector)
    {
        // 1. Walidacja wymiarów
        if (inputVector.Length != N)
        {
            throw new ArgumentException($"B³¹d wymiarów: Wektor wejœciowy ma rozmiar {inputVector.Length}, a macierz ma {N} kolumn. Musz¹ byæ równe.");
        }
        if (outputVector.Length != M)
        {
            throw new ArgumentException($"B³¹d wymiarów: Wektor wyjœciowy ma rozmiar {outputVector.Length}, a macierz ma {M} wierszy. Musz¹ byæ równe.");
        }
        if (matrix.Length != M * N)
        {
            throw new ArgumentException($"B³¹d wymiarów: Rozmiar tablicy macierzy ({matrix.Length}) jest zbyt ma³y dla zadeklarowanych wymiarów {M}x{N}.");
        }

        // 2. Mno¿enie macierzy przez wektor (zak³adamy uk³ad wierszowy / Row-Major)
        for (int i = 0; i < M; i++)
        {
            float sum = 0f;

            // Obliczamy offset dla danego wiersza raz, aby unikaæ mno¿enia w wewnêtrznej pêtli
            int rowOffset = i * N;

            for (int j = 0; j < N; j++)
            {
                // Odczyt matrix[rowOffset + j] jest sekwencyjny i niezwykle przyjazny dla CPU Cache
                sum += (float)matrix[rowOffset + j] * inputVector[j];
            }

            outputVector[i] = sum;
        }
    }

    private void VectorAddInPlace(float[] left, double[] right)
    {
        if (!(left.Length == right.Length))
        {
            throw new ArgumentException($"B³¹d wymiarów: Wektor lewy ma rozmiar {left.Length}, a prawy {right.Length}. Musz¹ byæ równe.");
        }

        for (int i = 0; i < left.Length; i++)
        {
            left[i] += (float)right[i];
        }
    }

    private void VectorReLUInPlace(float[] vector)
    {
        for (int i =0; i < vector.Length; i++)
        {
            vector[i] = Mathf.Max(vector[i], 0f);
        }
    }

    private void VectorTanhInPlace(float[] vector)
    {
        for (int i = 0; i < vector.Length; i++)
        {
            float e2x = Mathf.Exp(vector[i] * 2f);
            vector[i] = (e2x - 1f) / (e2x + 1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & WallLayerMask.value) != 0)
        {
            IsDeactivated = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
