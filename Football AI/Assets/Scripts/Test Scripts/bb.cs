using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bb : MonoBehaviour
{
    [System.NonSerialized]
    public int inputNodes;
    [System.NonSerialized]
    public int outputNodes;
    [System.NonSerialized]
    public int[] hiddenLayersNodes;
    [System.NonSerialized]
    public int[] LayersNodes;
    [System.NonSerialized]
    public float[][,] weightsNBiases;

    private void Start()
    {
        inputNodes = 2;
        outputNodes = 2;
        hiddenLayersNodes = new int[] { 2 };
        LayersNodes = new int[] { 2,2, 2 };

        weightsNBiases = new float[hiddenLayersNodes.Length + 1][,];

        for (int l = 0; l <= hiddenLayersNodes.Length; l++)
        {
            int rows = LayersNodes[l + 1];
            int cols = LayersNodes[l] + 1;
            weightsNBiases[l] = new float[rows, cols];

            for (int j = 0; j < rows; j++)
            {
                for (int k = 0; k < cols; k++)
                {
                    weightsNBiases[l][j, k] = -1;
                }
            }
        }

        float[] inputVec = new float[] {1,2};

        float[] outputArr = NeuralNetwork(inputVec);
        Debug.Log(outputArr[0].ToString() + " " + outputArr[1].ToString());
    }

    float leakyRelu(float input)
    {
        if (input > 0)
        {
            return input;
        }
        else
        {
            return 0.01f * input;
        }
    }

    float[] leakyRelu(float[] input)
    {
        float[] output = new float[input.Length + 1];
        output[0] = 1;

        for (int i = 1; i <= input.Length; i++)
        {
            output[i] = leakyRelu(input[i - 1]);
        }

        return output;
    }

    float[] matrixMult(float[,] matrix, float[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        float[] output = new float[rows];

        for (int i = 0; i < rows; i++)
        {
            output[i] = 0;
            for (int j = 0; j < cols; j++)
            {
                output[i] += matrix[i, j] * vector[j];
            }
        }

        return output;
    }

    float logisticAct(float input)
    {
        return 1 / (1 + Mathf.Exp(-input));
    }

    float[] logisticAct(float[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = logisticAct(input[i]);
        }
        return input;
    }

    float[] NeuralNetwork(float[] input)
    {
        float[] currentVector;
        currentVector = new float[input.Length + 1];
        currentVector[0] = 1;

        for (int i = 1; i <= input.Length; i++)
        {
            currentVector[i] = input[i - 1];
        }

        for (int i = 0; i < hiddenLayersNodes.Length; i++)
        {
            currentVector = matrixMult(weightsNBiases[i], currentVector);
            currentVector = leakyRelu(currentVector);
        }

        return logisticAct(matrixMult(weightsNBiases[hiddenLayersNodes.Length], currentVector));
    }
}
