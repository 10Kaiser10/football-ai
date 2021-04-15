using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class PlayerNeuralNet
{
    [System.NonSerialized]
    public static int inputNodes;
    [System.NonSerialized]
    public static int outputNodes;
    [System.NonSerialized]
    public static int[] hiddenLayersNodes;
    [System.NonSerialized]
    public static int[] LayersNodes;
    [System.NonSerialized]
    public static float[][,] weightsNBiases;

    public static void load()
    {
        SaveNN saver = Object.FindObjectOfType<SaveNN>().GetComponent<SaveNN>();
        saver.loadUp();
        inputNodes = saver.getInputNodes();
        outputNodes = saver.getoutputNodes();
        hiddenLayersNodes = saver.getHiddenNodes();
        LayersNodes = saver.getLayerNodes();
        weightsNBiases = saver.getwnbs();

        Debug.Log(inputNodes);
        Debug.Log(outputNodes);
    }

    static float[] matrixMult(float[,] matrix, float[] vector)
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

    static float logisticAct(float input)
    {
        return 1 / (1 + Mathf.Exp(-input));
    }

    static float[] logisticAct(float[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = logisticAct(input[i]);
        }
        return input;
    }

    public static float[] NeuralNetwork(float[] input)
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
            currentVector = logisticAct(currentVector);
        }

        return (matrixMult(weightsNBiases[hiddenLayersNodes.Length], currentVector));
    }
}
